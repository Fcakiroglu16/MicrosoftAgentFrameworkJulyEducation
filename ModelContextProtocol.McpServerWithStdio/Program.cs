using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.McpServerWithStdio.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMcpServer().WithStdioServerTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<TextContentTools>();

var app = builder.Build();

app.Run();