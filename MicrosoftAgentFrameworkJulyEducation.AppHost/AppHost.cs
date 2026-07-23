using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<App_API>("app-api");

builder.Build().Run();