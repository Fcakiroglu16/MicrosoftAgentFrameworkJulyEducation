using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<App_API>("app-api");

builder.AddProject<Projects.App2_API>("app2-api");

builder.Build().Run();