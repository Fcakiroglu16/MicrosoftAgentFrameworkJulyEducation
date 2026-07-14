using Microsoft.Extensions.AI;

namespace App.API.Endpoints;

internal static class ChatEndpoints
{
    internal static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/chat/smart", async (string message,
            [FromKeyedServices("smart")] IChatClient chatClient) =>
        {
            ChatResponse response = await chatClient.GetResponseAsync(message);
            return Results.Ok(new { model = "smart", reply = response.Text });
        });

        app.MapGet("/chat/fast", async (string message,
            [FromKeyedServices("fast")] IChatClient chatClient) =>
        {
            ChatResponse response = await chatClient.GetResponseAsync(message);
            return Results.Ok(new { model = "fast", reply = response.Text });
        });

        app.MapGet("api/chat", async (IChatClient chatClient, string message) =>
        {
            var response = await chatClient.GetResponseAsync(message);
            return response;
        });

        return app;
    }
}
