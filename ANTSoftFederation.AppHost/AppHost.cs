using SorteFederation.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.Build().Run();
