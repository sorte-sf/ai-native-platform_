using SorteFederation.AppHost.Bootstraping;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.Build().Run();
