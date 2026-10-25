using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SorteFederation.AppHost.Models;
using SorteFederation.AppHost.resources; 

namespace SorteFederation.AppHost.Bootstraping;

public static class ApplicationServiceRegistrations
{
    public static async Task<IDistributedApplicationBuilder> AddApplicationServices(this IDistributedApplicationBuilder builder)
    {

        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

        var postgresUsr = builder.AddParameter("postgres-user", secret: true);
        var postgresPwd = builder.AddParameter("postgres-password", secret: true);
        var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);
        var grafanaAdminUser = builder.AddParameter("grafana-admin-user");
        var grafanaAdminPassword = builder.AddParameter("grafana-admin-password", secret: true);

        var rabbitUser = builder.AddParameter("rabbitmq-user", secret: true);
        var rabbitPwd = builder.AddParameter("rabbitmq-password", secret: true);

        static int GetPort(IConfiguration configuration, string key, int fallback) => int.TryParse(configuration[key], out var port) && port is > 0 and <= 65535 ? port : fallback;

        var mailpit = builder.AddMailPit(name: "mailpit", httpPort: 8025, smtpPort: 1025)
                             .WithDataVolume("mailpit-data")
                             .ExcludeFromManifest()
                             ;

        var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
                                .WithArgs("--config.file=/etc/prometheus/prometheus.yml", "--web.enable-otlp-receiver")
                                .WithBindMount("./prometheus", "/etc/prometheus", isReadOnly: false)
                                .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http")
                                .WithLifetime(ContainerLifetime.Persistent)
                                .ExcludeFromManifest()
                                ;

        var grafana = builder.AddContainer("grafana", "grafana/grafana")
                             .WithBindMount("./grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: false)
                             .WithBindMount("./grafana/provisioning", "/etc/grafana/provisioning", isReadOnly: false)
                             .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
                             .WithEnvironment("GF_SECURITY_ADMIN_USER", grafanaAdminUser)
                             .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", grafanaAdminPassword)
                             .WaitFor(prometheus)
                             .WithLifetime(ContainerLifetime.Persistent)
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

        var cache = builder.AddRedis("cache")
                           .WithIconName("memory")
                           .WithRedisInsight(a =>
                           {
                               a.WithContainerName("redis-insight");
                           })
                           .WithRedisCommander(c =>
                           {
                               c.WithContainerName("redis-commander");
                           })
                           .WithLifetime(ContainerLifetime.Persistent)
                           ;

        var postgres = builder.AddPostgres("SorteMasterDatabase", userName: postgresUsr, password: postgresPwd, port: 5432)
                              .WithDataVolume(name: "sorte_masterdb_vol", isReadOnly: false)
                              .WithRepl()
                              .WithPgAdmin(pgAdmin =>
                              {
                                  pgAdmin.WithHostPort(5050);
                                  pgAdmin.WithContainerName("pgadmin");
                                  pgAdmin.WithUrlForEndpoint("http", url => url.DisplayText = "Postgres-Admin");
                                  pgAdmin.WithLifetime(ContainerLifetime.Persistent);
                              })
                              .WithImage("pgvector/pgvector", "pg17")
                              .WithEnvironment("TZ", "Europe/Turkey")
                              .OnConnectionStringAvailable(async (resource, evt, cancellationToken) =>
                              {
                                  var logger = evt.Services.GetRequiredService<ILogger<Program>>();
                                  logger.LogInformation("✅ PostgreSQL with pgvector ready: {Name}", resource.Name);
                              })
                              .WithLifetime(ContainerLifetime.Persistent)
                             ;

        var masterDb = postgres.AddDatabase("SortePortalDb2026");
        var builderDb = postgres.AddDatabase("SorteBuilderDb2026");
        var blobDb = postgres.AddDatabase("SorteBlob2026Db");

        var rabbitMqPort = GetPort(builder.Configuration, "RabbitMQ:Port", 5672);
        var messaging = builder.AddRabbitMQ("messaging", userName: rabbitUser, password: rabbitPwd, port: rabbitMqPort)
                               .WithManagementPlugin(15672)
                               .WithContainerName("sorte-rabbitmq")
                               .WithEnvironment("TZ", "Europe/Turkey")
                               .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
                               .WithEnvironment("RABBITMQ_DEFAULT_USER", rabbitUser)
                               .WithEnvironment("RABBITMQ_DEFAULT_PASSWORD", rabbitPwd)
                               .WithDataVolume("sorte-rabbitmq_vol")
                               .WithLifetime(ContainerLifetime.Persistent)
                              ;

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

        var realtimeService = builder.AddProject<Projects.SorteFederation_Realtime>("sorte-realtime-service")
                               .WithEndpoint("http", endpoint =>
                               { 
                                   endpoint.IsProxied = false;
                               })
                               .WithEnvironment(context =>
                               {
                                   context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
                                   context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
                                   context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey;

                                   context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser;
                                   context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd;
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

        var portalApi = builder.AddProject<Projects.SorteFederation_Portal>("sorte-portal-service")
                               .WithEndpoint("http", endpoint =>
                               {
                                   //  endpoint.Port = portalApiHttpPort;
                                   //   endpoint.TargetPort = portalApiHttpPort;
                                   endpoint.IsProxied = false;
                               })
                               .WithExternalHttpEndpoints()
                               .WithUrlForEndpoint("http", (e) =>
                               {
                                   return new() { Url = "/scalar", DisplayText = "Sorte Portal API Docs." };
                               })
                               .WithEnvironment(context =>
                               {
                                   context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = builder.Environment.EnvironmentName;
                                   context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
                                   context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
                                   context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey;

                                   context.EnvironmentVariables["POSTGRES_HOST"] = postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
                                   context.EnvironmentVariables["POSTGRES_PORT"] = postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
                                   context.EnvironmentVariables["POSTGRES_USER"] = postgres.Resource.UserNameParameter;
                                   context.EnvironmentVariables["POSTGRES_PASSWORD"] = postgres.Resource.PasswordParameter;
                                   context.EnvironmentVariables["POSTGRES_DATABASE"] = masterDb.Resource.DatabaseName;

                                   context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser;
                                   context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd;

                                   context.EnvironmentVariables["JwtSettings__Issuer"] = "sorte.portal";
                                   context.EnvironmentVariables["JwtSettings__Audience"] = "sorte";
                                   context.EnvironmentVariables["JwtSettings__SecretKey"] = jwtSigningKey;
                               })
                               .WithReference(masterDb).WaitFor(masterDb)
                               .WithReference(cache)
                               .WithReference(messaging).WaitFor(messaging)
                               .WithOtlpExporter()
                               .WithReference(realtimeService)
                            ;

        var ollama = builder.AddOllama("sorte-ollama", port: 11434)
                            .WithLifetime(ContainerLifetime.Persistent)
                            .WithDataVolume()
                            //.WithGPUSupport()
                            //.WithOpenWebUI()
                            .WithOtlpExporter()
                            ;

        var chatModel = ollama.AddModel("ollama-chat", "qwen3:8b");
        var embeddingModel = ollama.AddModel("ollama-embeddings", "qwen3-embedding:0.6b");

        var portalBuilder = builder.AddProject<Projects.SorteFederation_Builders>("sorte-portal-builder")
                            .WithEndpoint("http", endpoint =>
                            {
                                endpoint.IsProxied = false;
                            })
                            .WithExternalHttpEndpoints()
                            .WithUrlForEndpoint("http", (e) =>
                            {
                                return new() { Url = "/scalar", DisplayText = "Sorte Builder API Docs." };
                            })
                            .WithUrlForEndpoint("http", (e) =>
                            {
                                return new()
                                {
                                    Url = "/sorte-q",
                                    DisplayText = "Sorte TickerQ Dashboard."
                                };
                            })
                            .WithEnvironment(context =>
                            {
                                context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = builder.Environment.EnvironmentName;
                                context.EnvironmentVariables["Auth__Jwt__Issuer"] = "sorte.portal";
                                context.EnvironmentVariables["Auth__Jwt__Audience"] = "sorte";
                                context.EnvironmentVariables["Auth__Jwt__Key"] = jwtSigningKey;

                                context.EnvironmentVariables["POSTGRES_HOST"] = postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
                                context.EnvironmentVariables["POSTGRES_PORT"] = postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
                                context.EnvironmentVariables["POSTGRES_USER"] = postgres.Resource.UserNameParameter;
                                context.EnvironmentVariables["POSTGRES_PASSWORD"] = postgres.Resource.PasswordParameter;
                                context.EnvironmentVariables["POSTGRES_DATABASE"] = builderDb.Resource.DatabaseName;

                                context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser;
                                context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd;
                            })
                            .WithReference(builderDb).WaitFor(builderDb)
                            .WithReference(cache)
                            .WithReference(embeddingModel).WaitFor(embeddingModel)
                            .WithReference(chatModel).WaitFor(chatModel)
                            .WithReference(messaging).WaitFor(messaging)
                            .WithEnvironment("N8N__BaseUrl", n8n.GetEndpoint("n8n-ui"))
                            .WithEnvironment("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT", "http://prometheus:9090/api/v1/otlp/v1/metrics")
                            .WithEnvironment("OTEL_EXPORTER_OTLP_METRICS_PROTOCOL", "http/protobuf")
                            .WithOtlpExporter()
                            .WaitFor(prometheus)
                            .WithReference(mailpit)
                            .WithReference(realtimeService)
                         ;

        var blobservice = builder.AddProject<Projects.SorteFederation_BlobApi>("sorte-blob-service")
                                 .WithEndpoint("http", endpoint =>
                                 {
                                     // endpoint.Port = blobServiceHttpPort;
                                     //   endpoint.TargetPort = blobServiceHttpPort;
                                     endpoint.IsProxied = false;
                                 })
                                 .WithEnvironment(context =>
                                 {
                                     context.EnvironmentVariables["POSTGRES_HOST"] = postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
                                     context.EnvironmentVariables["POSTGRES_PORT"] = postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
                                     context.EnvironmentVariables["POSTGRES_USER"] = postgres.Resource.UserNameParameter;
                                     context.EnvironmentVariables["POSTGRES_PASSWORD"] = postgres.Resource.PasswordParameter;
                                     context.EnvironmentVariables["POSTGRES_DATABASE"] = blobDb.Resource.DatabaseName;

                                     context.EnvironmentVariables["RABBITMQ_DEFAULT_USER"] = rabbitUser;
                                     context.EnvironmentVariables["RABBITMQ_DEFAULT_PASSWORD"] = rabbitPwd;

                                 })
                                 .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
                                 .WithEnvironment("Auth__Jwt__Issuer", "sorte.portal")
                                 .WithEnvironment("Auth__Jwt__Audience", "sorte")
                                 .WithEnvironment("Auth__Jwt__Key", jwtSigningKey)
                                 .WithExternalHttpEndpoints()
                                 .WithUrlForEndpoint("http", (e) =>
                                 {
                                     return new() { Url = "/scalar", DisplayText = "Blob API Docs." };
                                 })
                                 .WithVolume("blob_contents", "/Contents", env: "BLOB_CONTENTS_PATH")
                                 .WithReference(messaging).WaitFor(messaging)
                                 .WithReference(blobDb).WaitFor(blobDb)
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

        portalBuilder.WithReference(blobservice).WaitFor(blobservice);

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
                             .WithReference(portalApi)
                             .WithReference(portalBuilder)
                             .WithReference(blobservice)
                             .WithReference(designStudioService)
                             .WithReference(realtimeService)
                             .WithOtlpExporter()
                              ;

        var studio = builder.AddExecutable("sorte-builder-studio", "npm.cmd", "../../clients/AntsoftBuilder", "run", "start:aspire")
               .WithReference(portalBuilder)
               .WaitFor(portalBuilder)
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
              .WithReference(portalApi)
              .WaitFor(portalApi)
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

        /* builder.AddProject<Projects.SorteFederation_PortalWeb>("sorte-portal-web")
                                    .WithEndpoint("http", endpoint =>
                                    {
                                        endpoint.IsProxied = false;
                                    })
                                    .WithExternalHttpEndpoints()
                                    .WithReference(cache)
                                    .WithVolume("data", "/data", env: "WEB_DATA_PATH")
                                    .WithOtlpExporter()
                                    ;*/

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
