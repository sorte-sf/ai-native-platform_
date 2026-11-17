
namespace SorteFederation.AppHost.Bootstraping;

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

        //var registry = builder.AddContainerRegistry("dockerhub", "docker.io");

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

        IResourceBuilder<RedisResource> cache = builder.AddRedis("cache").WithIconName("memory").WithRedisInsight(delegate (IResourceBuilder<RedisInsightResource> a)
        {
            a.WithContainerName("redis-insight");
        })
       .WithRedisCommander(delegate (IResourceBuilder<RedisCommanderResource> c)
       {
           c.WithContainerName("redis-commander");
       })
       .WithLifetime(ContainerLifetime.Persistent);

        IResourceBuilder<RabbitMQServerResource> messaging = RabbitMQBuilderExtensions
            .AddRabbitMQ(port: GetPort(builder.Configuration, "RabbitMQ:Port", 5672), builder: builder, name: "messaging", userName: rabbitUser, password: rabbitPwd)
            .WithManagementPlugin(15672)
            .WithContainerName("sorte-rabbitmq")
            .WithEnvironment("TZ", "Europe/Istanbul")
            .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", rabbitUser)
            .WithEnvironment("RABBITMQ_DEFAULT_PASSWORD", rabbitPwd)
            .WithDataVolume("sorte-rabbitmq_vol")
            .WithLifetime(ContainerLifetime.Persistent);
         
        var ownerEmail = builder.AddParameter("n8n-owner-email");
        var ownerPasswordHash = builder.AddParameter("n8n-owner-password-hash", secret: true);
        var n8nEncryptionKey = builder.AddParameter("n8n-encryption-key", secret: true);

        var n8n = builder.AddContainer("n8n", "n8nio/n8n", "latest")
                         .WithHttpEndpoint(port: 5678, targetPort: 5678, name: "n8n-ui")
                         .WithBindMount("./n8n-data", "/home/node/.n8n")
                         .WithEnvironment("N8N_HOST", "0.0.0.0")
                         .WithEnvironment("N8N_PORT", "5678")
                         .WithEnvironment("WEBHOOK_URL", "http://localhost:5678/")
                         .WithEnvironment("N8N_METRICS", "true")
                         .WithEnvironment("N8N_ENCRYPTION_KEY", n8nEncryptionKey)
                         .WithEnvironment("N8N_INSTANCE_OWNER_MANAGED_BY_ENV", "true")
                         .WithEnvironment("N8N_INSTANCE_OWNER_EMAIL", ownerEmail)
                         .WithEnvironment("N8N_INSTANCE_OWNER_PASSWORD_HASH", ownerPasswordHash)
                         .WithLifetime(ContainerLifetime.Persistent)
                         ;

        IResourceBuilder<ProjectResource> realtimeService = builder.AddProject<SorteFederation_Realtime>("sorte-realtime-service")
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

        IResourceBuilder<ProjectResource> paymentService = builder.AddProject<SorteFederation_PaymentGateway>("sorte-payment-service")
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

        IResourceBuilder<OllamaResource> chatModel = builder.AddOllama("sorte-platform-ollama", 11434)
             .WithLifetime(ContainerLifetime.Persistent).WithDataVolume()
             .WithOtlpExporter();

        var ollamaChatModel = chatModel.AddModel("ollama-chat", "qwen3:8b");
        var embeddingModel = chatModel.AddModel("ollama-embeddings", "qwen3-embedding:0.6b");

        var portalPostgres = builder.AddPostgres("sorte-platform-portal-db", builder.ExecutionContext.IsPublishMode ? postgresUsr : null, builder.ExecutionContext.IsPublishMode ? postgresPwd : null).WithPgAdmin()
            .WithImage("pgvector/pgvector", "pg18")
            .WithVolume("sorte-platform-portal-data", "/var/lib/postgresql")
            .WithLifetime(ContainerLifetime.Persistent);

        var portalDb = portalPostgres.AddDatabase("SortePlatformDb2027");

        IResourceBuilder<ProjectResource> portalService = builder.AddProject<SorteFederation_Portal>("sorte-portal-service").WithHttpHealthCheck("/health", endpointName: "http").WithEndpoint("http", delegate (EndpointAnnotation endpoint)
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

        IResourceBuilder<ProjectResource> platformBuilderService = builder.AddProject<Projects.SorteFederation_Builders>("sorte-portal-builder")
            .WithHttpHealthCheck("/health", endpointName: "http")
            .WithEndpoint("http", delegate (EndpointAnnotation endpoint)
        {
            endpoint.IsProxied = false;
        }).WithExternalHttpEndpoints()
            .WithUrlForEndpoint("http", (EndpointReference e) => new ResourceUrlAnnotation
            {
                Url = "/scalar",
                DisplayText = "Sorte Builder API Docs."
            })
            .WithUrlForEndpoint("http", (EndpointReference e) => new ResourceUrlAnnotation
            {
                Url = "/hangfire",

                DisplayText = "Sorte Hangfire Docs."
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

        IResourceBuilder<ProjectResource> platformBlobService = builder.AddProject<Projects.SorteFederation_BlobApi>("sorte-blob-builder")
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


        var designStudioService = builder.AddProject<Projects.SorteFederation_DesignerStudio>("sorte-designer-studio-service")
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

        var gateway = builder.AddProject<Projects.SorteFederation_LoadBalancing>("sorte-gateway")
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

        var studio = builder.AddExecutable("sorte-builder-studio", "npm.cmd", "../../clients/AntsoftBuilder", "run", "start:aspire")
               .WithReference(portalService).WaitFor(portalService)
               .WithHttpEndpoint(port: 4202, targetPort: 4202, isProxied: false)
               .WithExternalHttpEndpoints()
               .ExcludeFromManifest()
            ;

        var designStudioUi = builder.AddExecutable("sorte-design-studio-ui", "npm.cmd", "../../clients/ANTSoftDesignStudio", "run", "start:aspire")
               .WithReference(gateway)
               .WaitFor(gateway)
               .WithReference(designStudioService)
               .WaitFor(designStudioService)
               .WithHttpEndpoint(port: 4203, targetPort: 4203, isProxied: false)
               .WithExternalHttpEndpoints()
               .ExcludeFromManifest()
               ;

        var portalUi = builder.AddExecutable("sorte-portal-ui", "npm.cmd", "../../clients/ANTSoftPortal", "run", "start:aspire")
              .WithReference(gateway)
              .WaitFor(gateway)
              .WithReference(portalService)
              .WaitFor(portalService)
              .WithHttpEndpoint(port: 4204, targetPort: 4204, isProxied: false)
              .WithExternalHttpEndpoints()
              .ExcludeFromManifest()
              ;

        builder.AddProject<Projects.SorteFederation_WebSite>("sorte-web-site")
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

        builder.AddProject<Projects.SorteFederation_PortalWeb>("sorte-portal-web")
                                    .WithEndpoint("http", endpoint =>
                                    {
                                        endpoint.IsProxied = false;
                                    })
                                    .WithExternalHttpEndpoints()
                                    .WithReference(cache)
                                    .WithVolume("data", "/data", env: "WEB_DATA_PATH")
                                    .WithOtlpExporter()
                                    ;

        /* var options = new DevTunnelOptions
         {
             Description = "QA environment tunnel",
             Labels = ["qa", "testing"],
             AllowAnonymous = true,
         };

         if (string.Equals(Environment.GetEnvironmentVariable("SORTE_ENABLE_DEV_TUNNEL"), "true", StringComparison.OrdinalIgnoreCase))
         {
             var tunnelId = Guid.CreateVersion7().ToString("N");
             var tunnel = builder.AddDevTunnel("antsoft-tunnel", options: options);
             if (string.Equals(Environment.GetEnvironmentVariable("SORTE_ENABLE_DEV_TUNNEL"), "true", StringComparison.OrdinalIgnoreCase))
             {
                 tunnel.WithAnonymousAccess();
             }

             tunnel.WithReference(studio).WaitFor(studio);
             tunnel.WithReference(portalweb).WaitFor(portalweb);

             tunnel.WithReference(portalApi).WaitFor(portalApi);
             tunnel.WithReference(portalBuilder).WaitFor(portalBuilder);

             tunnel.WithReference(designStudioService).WaitFor(designStudioService);
         }*/

        return builder;

    }
}
