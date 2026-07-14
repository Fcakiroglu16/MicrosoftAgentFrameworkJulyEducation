using Microsoft.Extensions.AI;

namespace App.API.Endpoints;

internal static class ChatbotEndpoints
{
    internal static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/chatbot", async (string message,
            [FromKeyedServices("smart")] IChatClient chatClient) =>
        {
            return Results.Ok();
        });


        return app;
    }
}
