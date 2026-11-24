namespace SorteFederation.AppHost;

public static class ApplicationServiceRegistrations
{
    public static async Task<IDistributedApplicationBuilder> AddApplicationServices(this IDistributedApplicationBuilder builder)
    {

        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

        IResourceBuilder<ParameterResource> postgresUsr = builder.AddParameter("postgres-user", secret: true);
        IResourceBuilder<ParameterResource> postgresPwd = builder.AddParameter("postgres-password", secret: true);
        IResourceBuilder<ParameterResource> jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);

        IResourceBuilder<ParameterResource> rabbitUser = builder.AddParameter("rabbitmq-user", secret: true);
        IResourceBuilder<ParameterResource> rabbitPwd = builder.AddParameter("rabbitmq-password", secret: true);

        static int GetPort(IConfiguration configuration, string key, int fallback) => int.TryParse(configuration[key], out var port) && port is > 0 and <= 65535 ? port : fallback;

        var mailpit = builder.AddMailPit(name: "mailpit", httpPort: 8025, smtpPort: 1025)
                             .WithDataVolume("mailpit-data")
                             .ExcludeFromManifest()
                             ;

        var dockerComposeEnv = builder.AddDockerComposeEnvironment("compose")
                 .WithProperties(env =>
                 {
                     env.DefaultNetworkName = "sorte";
                     env.DashboardEnabled = true;
                 })
                 .WithIconName("cloud")
                 .WithDashboard(dashboard =>
                 {
                     dashboard.WithHostPort(8080).WithForwardedHeaders(enabled: true);
                 })
                 .WithExplicitStart()
                 //.WithSshDeploySupport()
                 //.WithAppFileTransfer("certs", "certs")
                 ;

        dockerComposeEnv.WithDashboard(dashboard =>
        {
            dashboard.WithHostPort(GetPort(builder.Configuration, "ASPIRE_DASHBOARD_PORT", 8080)).WithForwardedHeaders(enabled: true);
        });

        var externalApiSettings = builder.Configuration.GetSection("ExternalAPiSettings").Get<ExternalServiceUrl>();

        IResourceBuilder<ExternalServiceResource>? figmaExternalService = null;
        if (externalApiSettings?.Enabled == true)
        {
            foreach (var (configuredApi, index) in externalApiSettings.Apis.Select((configuredApi, index) => (configuredApi, index)))
            {
                var endpoint = $"{configuredApi.BaseUrl.TrimEnd('/')}/{configuredApi.ApiUrl.TrimStart('/')}";
                var name = configuredApi.Name ?? $"external-service-{index + 1}";

                var externalService = builder.AddExternalService(name.Trim(), endpoint);

                if (string.Equals(name.Trim(), "Figma-Api", StringComparison.OrdinalIgnoreCase))
                {
                    figmaExternalService = externalService;
                }
            }
        }

        var cache = builder.AddRedis("cache").WithIconName("memory").WithRedisInsight(delegate (IResourceBuilder<RedisInsightResource> a)
        {
            a.WithContainerName("redis-insight");
        })
       .WithRedisCommander(delegate (IResourceBuilder<RedisCommanderResource> c)
       {
           c.WithContainerName("redis-commander");
       })
       .WithLifetime(ContainerLifetime.Persistent);

        var messaging = RabbitMQBuilderExtensions
            .AddRabbitMQ(port: GetPort(builder.Configuration, "RabbitMQ:Port", 5672), builder: builder, name: "messaging", userName: rabbitUser, password: rabbitPwd)
            .WithManagementPlugin(15672)
            .WithContainerName("sorte-rabbitmq")
            .WithEnvironment("TZ", "Europe/Istanbul")
            .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", rabbitUser)
            .WithEnvironment("RABBITMQ_DEFAULT_PASSWORD", rabbitPwd)
            .WithDataVolume("sorte-rabbitmq_vol")
            .WithLifetime(ContainerLifetime.Persistent);

        var realtimeService = builder.AddDotnetProject("sorte-realtime-service", "../ANTSoftFederation.Realtime/SorteFederation.Realtime.csproj")
            .WithHttpHealthCheck("/health", endpointName: "http")
            .WithEndpoint("http", delegate (EndpointAnnotation endpoint)
        {
            endpoint.IsProxied = false;
        }).WithEnvironment(delegate (EnvironmentCallbackContext context)
        {
            context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
            context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
            context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey.Resource;
            context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser.Resource;
            context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd.Resource;
        })
        .WithReference(messaging)
        .WaitFor(messaging)
        .WithOtlpExporter()
        ;

        var paymentService = builder.AddDotnetProject("sorte-payment-service", "../ANTSoftFederation.PaymentGateway/SorteFederation.PaymentGateway.csproj")
          .WithHttpHealthCheck("/health", endpointName: "http")
          .WithEndpoint("http", delegate (EndpointAnnotation endpoint)
          {
              endpoint.IsProxied = false;
          })
          .WithUrlForEndpoint("http", (e) =>
          {
              return new() { Url = "/scalar", DisplayText = "Payment gateway API Docs." };
          })
         .WithEnvironment(delegate (EnvironmentCallbackContext context)
          {
              context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
              context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
              context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey.Resource;
              context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser.Resource;
              context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd.Resource;
          })
          .WithReference(messaging)
          .WaitFor(messaging)
          .WithOtlpExporter()
          ;

        var portainerAgentToken = builder.AddParameter("portainer-agent-token", secret: true);
        var portainer = builder.AddContainer("portainer", "portainer/portainer-ce", "lts")
                               .WithHttpEndpoint(targetPort: 9000, port: 9000, name: "portainer-http")
                               .WithHttpEndpoint(targetPort: 9443, port: 9443, name: "portainer-https")
                               .WithVolume("portainer_data", "/data")
                               .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock")
                               // .WithEnvironment("AGENT_SECRET", portainerAgentToken)
                               // .WithEnvironment("setup-token", portainerAgentToken) 
                               .WithLifetime(ContainerLifetime.Persistent)
                               ;

        var chatModel = builder.AddOllama("sorte-platform-ollama", 11434)
             .WithLifetime(ContainerLifetime.Persistent).WithDataVolume()
             .WithOtlpExporter();

        var ollamaChatModel = chatModel.AddModel("ollama-chat", "qwen3:8b");
        var embeddingModel = chatModel.AddModel("ollama-embeddings", "qwen3-embedding:0.6b");

        var portalPostgres = builder.AddPostgres("sorte-platform-portal-db", builder.ExecutionContext.IsPublishMode ? postgresUsr : null, builder.ExecutionContext.IsPublishMode ? postgresPwd : null).WithPgAdmin()
            .WithImage("pgvector/pgvector", "pg18")
            .WithVolume("sorte-platform-portal-data", "/var/lib/postgresql")
            .WithLifetime(ContainerLifetime.Persistent);

        var portalDb = portalPostgres.AddDatabase("SortePlatformDb2027");

        var portalService = builder.AddDotnetProject("sorte-portal-service", "../ANTSoftFederation.Portal/SorteFederation.Portal.csproj")
            .WithHttpHealthCheck("/health", endpointName: "http")
            .WithEndpoint("http", delegate (EndpointAnnotation endpoint)
        {
            endpoint.IsProxied = false;
        }).WithExternalHttpEndpoints()
            .WithUrlForEndpoint("http", (EndpointReference e) => new ResourceUrlAnnotation
            {
                Url = "/scalar",
                DisplayText = "Sorte Portal API Docs."
            })
            .WithEnvironment(delegate (EnvironmentCallbackContext context)
            {
                context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = builder.Environment.EnvironmentName;
                context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
                context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
                context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey.Resource;
                context.EnvironmentVariables["POSTGRES_HOST"] = portalPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
                context.EnvironmentVariables["POSTGRES_PORT"] = portalPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
                context.EnvironmentVariables["POSTGRES_USER"] = postgresUsr;
                context.EnvironmentVariables["POSTGRES_PASSWORD"] = postgresPwd;
                context.EnvironmentVariables["POSTGRES_DATABASE"] = portalDb.Resource.DatabaseName;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser.Resource;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd.Resource;
                context.EnvironmentVariables["JwtSettings__Issuer"] = "sorte.portal";
                context.EnvironmentVariables["JwtSettings__Audience"] = "sorte";
                context.EnvironmentVariables["JwtSettings__SecretKey"] = jwtSigningKey.Resource;
            })
            .WithReference(portalDb)
            .WaitFor(portalPostgres)
            .WithReference(cache)
            .WaitFor(cache)
            .WithReference(messaging)
            .WaitFor(messaging)
            .WithOtlpExporter()
            .WithReference(realtimeService)
            ;

        var platformBuilderPostgres = builder.AddPostgres("sorte-platform-builder-db", builder.ExecutionContext.IsPublishMode ? postgresUsr : null, builder.ExecutionContext.IsPublishMode ? postgresPwd : null).WithPgAdmin()
            .WithImage("pgvector/pgvector", "pg18")
            .WithVolume("sorte-platform-builder-data", "/var/lib/postgresql").WithLifetime(ContainerLifetime.Persistent);

        var platformBuilderDb = platformBuilderPostgres.AddDatabase("SortePlatformBuilderDb2027");

        var platformBuilderService = builder.AddDotnetProject("sorte-portal-builder", "../ANTSoftFederation.Builders/SorteFederation.Builders.csproj")
            .WithHttpHealthCheck("/health", endpointName: "http")
            .WithEndpoint("http", delegate (EndpointAnnotation endpoint)
            {
                endpoint.IsProxied = false;
            }).WithExternalHttpEndpoints()
            .WithUrlForEndpoint("http", url =>
            {
                url.Url = "/hangfire";
                url.DisplayText = "Sorte Hangfire Dashboard.";
            })
            .WithUrlForEndpoint("http", (EndpointReference e) => new ResourceUrlAnnotation
            {
                Url = "/scalar",
                DisplayText = "Sorte Builder API Docs."
            })

            .WithEnvironment(delegate (EnvironmentCallbackContext context)
            {
                context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = builder.Environment.EnvironmentName;
                context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
                context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
                context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey.Resource;
                context.EnvironmentVariables["POSTGRES_HOST"] = platformBuilderPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
                context.EnvironmentVariables["POSTGRES_PORT"] = platformBuilderPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
                context.EnvironmentVariables["POSTGRES_USER"] = postgresUsr;
                context.EnvironmentVariables["POSTGRES_PASSWORD"] = postgresPwd;
                context.EnvironmentVariables["POSTGRES_DATABASE"] = platformBuilderDb.Resource.DatabaseName;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser.Resource;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd.Resource;
                context.EnvironmentVariables["JwtSettings__Issuer"] = "sorte.portal";
                context.EnvironmentVariables["JwtSettings__Audience"] = "sorte";
                context.EnvironmentVariables["JwtSettings__SecretKey"] = jwtSigningKey.Resource;
            })
            .WithReference(platformBuilderDb)
            .WithReference(ollamaChatModel)
            .WithReference(embeddingModel)
            .WaitFor(platformBuilderPostgres)
            .WithReference(cache)
            .WaitFor(cache)
            .WithReference(messaging)
            .WaitFor(messaging)
            .WithOtlpExporter()
            .WithReference(realtimeService)
            ;

        var platformBlobPostgres = builder.AddPostgres("sorte-platform-blob-db",
            builder.ExecutionContext.IsPublishMode ? postgresUsr : null, builder.ExecutionContext.IsPublishMode ? postgresPwd : null)
            .WithPgAdmin()
            .WithImage("pgvector/pgvector", "pg18")
            .WithVolume("sorte-platform-blob-data", "/var/lib/postgresql")
            .WithLifetime(ContainerLifetime.Persistent);

        var blobDb = platformBlobPostgres.AddDatabase("SortePlatformBlobDb2027");

        var platformBlobService = builder.AddDotnetProject("sorte-blob-builder", "../ANTSoftFederation.BlobApi/SorteFederation.BlobApi.csproj")
            .WithHttpHealthCheck("/health", endpointName: "http")
            .WithEndpoint("http", delegate (EndpointAnnotation endpoint)
            {
                endpoint.IsProxied = false;
            }).WithExternalHttpEndpoints()
            .WithUrlForEndpoint("http", (EndpointReference e) => new ResourceUrlAnnotation
            {
                Url = "/scalar",
                DisplayText = "Sorte Blob API Docs."
            })
            .WithEnvironment(delegate (EnvironmentCallbackContext context)
            {
                context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = builder.Environment.EnvironmentName;
                context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
                context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
                context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey.Resource;
                context.EnvironmentVariables["POSTGRES_HOST"] = platformBlobPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
                context.EnvironmentVariables["POSTGRES_PORT"] = platformBlobPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
                context.EnvironmentVariables["POSTGRES_USER"] = postgresUsr;
                context.EnvironmentVariables["POSTGRES_PASSWORD"] = postgresPwd;
                context.EnvironmentVariables["POSTGRES_DATABASE"] = blobDb.Resource.DatabaseName;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser.Resource;
                context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd.Resource;
                context.EnvironmentVariables["JwtSettings__Issuer"] = "sorte.portal";
                context.EnvironmentVariables["JwtSettings__Audience"] = "sorte";
                context.EnvironmentVariables["JwtSettings__SecretKey"] = jwtSigningKey.Resource;
            })
            .WithReference(blobDb)
            .WaitFor(platformBlobPostgres)
            .WithReference(cache)
            .WaitFor(cache)
            .WithReference(messaging)
            .WaitFor(messaging)
            .WithOtlpExporter()
            ;


        var designStudioService = builder.AddDotnetProject("sorte-designer-studio-service", "../ANTSoftFederation.DesignerStudio/SorteFederation.DesignerStudio.csproj")
                                 .WithEndpoint("http", endpoint =>
                                 {
                                     endpoint.IsProxied = false;
                                 })
                                 .WithEnvironment("Auth__Jwt__Issuer", "sorte.portal")
                                 .WithEnvironment("Auth__Jwt__Audience", "sorte")
                                 .WithEnvironment("Auth__Jwt__Key", jwtSigningKey)
                                 .WithExternalHttpEndpoints()
                                 .WithUrlForEndpoint("http", (e) =>
                                 {
                                     return new() { Url = "/scalar", DisplayText = "Designer Studio API Docs." };
                                 })
                                 .WithVolume("designer_studio_template_db", "/Contents/Data", env: "DESIGNER_TEMPLATE_PACKAGES_PATH")
                                 .WithReference(messaging).WaitFor(messaging)
                                 .WithOtlpExporter()
                                 .WithReference(realtimeService)
                              ;

        if (figmaExternalService is not null)
        {
            designStudioService.WithReference(figmaExternalService);
        }

        var gateway = builder.AddDotnetProject("sorte-gateway", "../ANTSoftFederation.LoadBalancing/SorteFederation.LoadBalancing.csproj")
                             .WithEndpoint("http", endpoint =>
                             {
                                 endpoint.IsProxied = false;
                             })
                             .WithExternalHttpEndpoints()
                             .WithEnvironment("JwtSettings__Issuer", "sorte.portal")
                             .WithEnvironment("JwtSettings__Audience", "sorte")
                             .WithEnvironment("JwtSettings__SecretKey", jwtSigningKey)
                             .WithUrlForEndpoint("http", url => url.DisplayText = "Gateway-Service")
                             .WithReference(portalService)
                             .WithReference(platformBuilderService)
                             .WaitFor(platformBuilderService)
                             .WithReference(platformBlobService)
                             .WithReference(designStudioService)
                             .WithReference(realtimeService)
                             .WithOtlpExporter()
                              ;

        builder.AddDotnetProject("sorte-web-site", "../ANTSoftFederation.WebSite/SorteFederation.WebSite.csproj")
                                   .WithEndpoint("http", endpoint =>
                                   {
                                       endpoint.IsProxied = false;
                                   })
                                   .WithExternalHttpEndpoints()
                                   .WithEnvironment("POSTGRES_USER", postgresUsr)
                                   .WithReference(cache)
                                   .WithVolume("data", "/data", env: "WEB_DATA_PATH")
                                   .WithOtlpExporter()
                                   ;

        builder.AddDotnetProject("sorte-portal-web", "../ANTSoftFederation.PortalWeb/SorteFederation.PortalWeb.csproj")
                                    .WithEndpoint("http", endpoint =>
                                    {
                                        endpoint.IsProxied = false;
                                    })
                                    .WithExternalHttpEndpoints()
                                    .WithReference(cache)
                                    .WithVolume("data", "/data", env: "WEB_DATA_PATH")
                                    .WithOtlpExporter()
                                    ;

        var npmCommand = OperatingSystem.IsWindows() ? "npm.cmd" : "npm";

        var studio = builder.AddExecutable("sorte-builder-studio", npmCommand, "../../Clients/AntsoftBuilder", "run", "start:aspire")
               .WithReference(gateway)
               .WaitFor(gateway)
               .WithReference(portalService).WaitFor(portalService)
               .WithHttpEndpoint(port: 4202, targetPort: 4202, isProxied: false)
               .WithExternalHttpEndpoints()
               .ExcludeFromManifest()
            ;

        var designStudioUi = builder.AddExecutable("sorte-design-studio-ui", npmCommand, "../../Clients/ANTSoftDesignStudio", "run", "start:aspire")
               .WithReference(gateway)
               .WaitFor(gateway)
               .WithReference(designStudioService)
               .WaitFor(designStudioService)
               .WithHttpEndpoint(port: 4203, targetPort: 4203, isProxied: false)
               .WithExternalHttpEndpoints()
               .ExcludeFromManifest()
               ;

        var portalUi = builder.AddExecutable("sorte-portal-ui", npmCommand, "../../Clients/ANTSoftPortal", "run", "start:aspire")
              .WithReference(gateway)
              .WaitFor(gateway)
              .WithReference(portalService)
              .WaitFor(portalService)
              .WithHttpEndpoint(port: 4204, targetPort: 4204, isProxied: false)
              .WithExternalHttpEndpoints()
              .ExcludeFromManifest()
              ;

        return builder;

    }
}
