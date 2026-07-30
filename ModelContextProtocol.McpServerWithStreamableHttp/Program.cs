using ModelContextProtocol.McpServerWithStreamableHttp.Prompts;
using ModelContextProtocol.McpServerWithStreamableHttp.Resources;
using ModelContextProtocol.McpServerWithStreamableHttp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer().WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<TextContentTools>()
    .WithResources<UserProfileResources>()
    .WithPrompts<BasicPrompts>()
    .WithPrompts<CodeAssistantPrompts>();

    
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapMcp();
app.Run();

