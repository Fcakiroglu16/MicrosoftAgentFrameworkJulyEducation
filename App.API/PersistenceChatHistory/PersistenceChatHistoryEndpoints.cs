using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace App.API.PersistenceChatHistory;

public static class PersistenceChatHistoryEndpoints
{
    /// <summary>
    ///     Maps the /chat-with-persistence endpoint backed by the EF Core persisted chat history agent.
    /// </summary>
    public static IEndpointRouteBuilder MapPersistenceChatHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat-with-persistence", async Task<Results<Ok<ChatResponse>, BadRequest<string>>>
        (ChatRequest request, [FromKeyedServices("agent-with-persistence")] AIAgent agent,
            ConversationContext conversationContext, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return TypedResults.BadRequest("message is required.");
            }

            // 1. Client bir conversationId gönderdiyse onu kullan, göndermediyse yeni bir tane oluştur.
            var conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
                ? Guid.NewGuid().ToString()
                : request.ConversationId;

            // 2. Singleton agent'ın ChatHistoryProvider'ı ConversationId'yi buradan okuyacak.
            conversationContext.ConversationId = conversationId;

            // 3. Her istekte yeni bir (boş) AgentSession oluştur; geçmiş mesajlar zaten
            // EfCoreChatHistoryProvider tarafından conversationId üzerinden DB'den yüklenecek.
            var session = await agent.CreateSessionAsync(cancellationToken);

            // 4. Agent'ı bu objeyle çalıştır
            var response = await agent.RunAsync(request.Message, session, cancellationToken: cancellationToken);

            return TypedResults.Ok(new ChatResponse(conversationId, response.Text));
        });

        return app;
    }
}
