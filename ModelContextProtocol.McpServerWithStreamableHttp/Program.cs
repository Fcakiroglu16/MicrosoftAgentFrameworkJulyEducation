using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.McpServerWithStreamableHttp.Data;
using ModelContextProtocol.McpServerWithStreamableHttp.Prompts;
using ModelContextProtocol.McpServerWithStreamableHttp.Resources;
using ModelContextProtocol.McpServerWithStreamableHttp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));

builder.Services.AddMcpServer().WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<TextContentTools>()
    .WithTools<ProductTools>()
    .WithResources<UserProfileResources>()
    .WithPrompts<BasicPrompts>()
    .WithPrompts<CodeAssistantPrompts>();

       
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapMcp("/mcp");
app.Run();

