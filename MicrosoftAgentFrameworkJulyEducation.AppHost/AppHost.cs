using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var app2 = builder.AddProject<Projects.App2_API>("app2-api");

builder.AddProject<App_API>("app-api")
    .WithReference(app2)
    .WaitFor(app2);

builder.Build().Run();