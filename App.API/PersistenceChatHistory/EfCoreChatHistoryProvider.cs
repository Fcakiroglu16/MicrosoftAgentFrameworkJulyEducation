using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace App.API.PersistenceChatHistory;

public sealed class EfCoreChatHistoryProvider : ChatHistoryProvider
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ProviderSessionState<State> _sessionState;

    public EfCoreChatHistoryProvider(IServiceProvider serviceProvider, string? stateKey = null)
    {
        _serviceProvider = serviceProvider;
        _sessionState = new ProviderSessionState<State>(
            _ => new State
            {
                ConversationId = ResolveConversationId() ?? Guid.NewGuid().ToString()
            },
            stateKey ?? nameof(EfCoreChatHistoryProvider));
    }


    private string? ResolveConversationId()
    {
        return _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?
            .RequestServices.GetService<ConversationContext>()?.ConversationId;
    }


    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        var state = _sessionState.GetOrInitializeState(context.Session);

        // Scope to access EF Core DbContext
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatHistoryDbContext>();

        var dbState = await dbContext.ChatSessionStates.FindAsync([state.ConversationId], cancellationToken);
        if (dbState == null) return [];
        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(dbState.MessagesJson);
        return messages ?? [];

    }

    protected override async ValueTask StoreChatHistoryAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        var state = _sessionState.GetOrInitializeState(context.Session);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatHistoryDbContext>();

        var dbState = await dbContext.ChatSessionStates.FindAsync([state.ConversationId], cancellationToken);

        List<ChatMessage> existingMessages = new();
        if (dbState != null)
        {
            existingMessages = JsonSerializer.Deserialize<List<ChatMessage>>(dbState.MessagesJson) ?? [];
        }
        else
        {
            dbState = new ChatSessionState { ConversationId = state.ConversationId };
            dbContext.ChatSessionStates.Add(dbState);
        }

        var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []).ToList();
        existingMessages.AddRange(allNewMessages);

        dbState.MessagesJson = JsonSerializer.Serialize(existingMessages);
        await dbContext.SaveChangesAsync(cancellationToken);

        _sessionState.SaveState(context.Session, state);
    }

    private sealed class State
    {
        public string ConversationId { get; set; } = string.Empty;
    }
}