using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;

namespace App.API.InMemoryChatHistory;

public static class InMemoryChatHistoryEndpoints
{
    /// <summary>
    /// Maps the /chat and /chat/{conversationId}/history endpoints backed by an in-memory chat history agent.
    /// </summary>
    public static IEndpointRouteBuilder MapInMemoryChatHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat", async Task<Results<Ok<ChatResponse>, BadRequest<string>>>
            (ChatRequest request, AIAgent agent, IMemoryCache cache, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return TypedResults.BadRequest("message is required.");
            }

            var conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
                ? Guid.NewGuid().ToString()
                : request.ConversationId;

            if (!cache.TryGetValue<AgentSession>(conversationId, out var session) || session is null)
            {
                session = await agent.CreateSessionAsync(cancellationToken);
            }

            var response = await agent.RunAsync(request.Message, session, cancellationToken: cancellationToken);

            cache.Set(conversationId, session, TimeSpan.FromMinutes(30));

            return TypedResults.Ok(new ChatResponse(conversationId, response.Text));
        });

        app.MapGet("/chat/{conversationId}/history", Results<Ok<IReadOnlyList<ChatMessageDto>>, NotFound<string>>
            (string conversationId, AIAgent agent, IMemoryCache cache) =>
        {
            if (!cache.TryGetValue<AgentSession>(conversationId, out var session) || session is null)
            {
                return TypedResults.NotFound($"No conversation found for id '{conversationId}'.");
            }

            var provider = agent.GetService<InMemoryChatHistoryProvider>();
            var messages = provider?.GetMessages(session) ?? [];

            var history = messages
                .Select(m => new ChatMessageDto(m.Role.Value, m.Text))
                .ToArray();

            return TypedResults.Ok<IReadOnlyList<ChatMessageDto>>(history);
        });

        return app;
    }
}
