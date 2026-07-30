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

var apiKey = app.Configuration["ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException(
        "ApiKey yapılandırması eksik. appsettings.json, ortam değişkeni (ApiKey) veya user-secrets ile bir değer tanımlayın.");
}

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/mcp"),
    branch => branch.Use(async (context, next) =>
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey) ||
            !string.Equals(providedKey, apiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: geçersiz veya eksik API anahtarı.");
            return;
        }

        await next();
    }));

app.MapMcp("/mcp");
app.Run();

