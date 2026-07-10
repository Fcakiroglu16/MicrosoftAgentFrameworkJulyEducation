var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.App_API>("app-api");

builder.Build().Run();
