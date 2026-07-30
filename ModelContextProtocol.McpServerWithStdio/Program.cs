using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.McpServerWithStdio.Prompts;
using ModelContextProtocol.McpServerWithStdio.Resources;
using ModelContextProtocol.McpServerWithStdio.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMcpServer().WithStdioServerTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<TextContentTools>()
    .WithResources<UserProfileResources>()
    .WithPrompts<BasicPrompts>()
    .WithPrompts<CodeAssistantPrompts>();

var app = builder.Build();

app.Run();