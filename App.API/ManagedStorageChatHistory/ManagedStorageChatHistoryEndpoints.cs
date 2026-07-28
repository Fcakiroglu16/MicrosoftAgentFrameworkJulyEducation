using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace App.API.ManagedStorageChatHistory;

public static class ManagedStorageChatHistoryEndpoints
{
    /// <summary>
    ///     Maps the /chat-with-managed-storage endpoint backed by the OpenAI service-managed chat history agent.
    /// </summary>
    public static IEndpointRouteBuilder MapManagedStorageChatHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat-with-managed-storage", async Task<Results<Ok<ChatResponse>, BadRequest<string>>>
            (ChatRequest request, [FromKeyedServices("agent-with-managed-storage")] AIAgent agent,
                CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return TypedResults.BadRequest("message is required.");
            }

            var chatAgent = (ChatClientAgent)agent;

 
            var session = string.IsNullOrWhiteSpace(request.ConversationId)
                ? await chatAgent.CreateSessionAsync(cancellationToken)
                : await chatAgent.CreateSessionAsync(request.ConversationId);

            var response = await chatAgent.RunAsync(request.Message, session, cancellationToken: cancellationToken);

            var conversationId = (session as ChatClientAgentSession)?.ConversationId ?? string.Empty;

            return TypedResults.Ok(new ChatResponse(conversationId, response.Text));
        });

        return app;
    }
}
