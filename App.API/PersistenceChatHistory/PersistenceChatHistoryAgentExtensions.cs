using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace App.API.PersistenceChatHistory;

public static class PersistenceChatHistoryAgentExtensions
{
    /// <summary>
    ///     Registers the EF Core chat history persistence infrastructure and a keyed
    ///     "agent-with-persistence" <see cref="AIAgent" /> whose <see cref="EfCoreChatHistoryProvider" />
    ///     resolves the ConversationId from the scoped <see cref="ConversationContext" /> at request time.
    /// </summary>
    public static IServiceCollection AddPersistenceChatHistoryAgent(this IServiceCollection services)
    {
        services.AddDbContext<ChatHistoryDbContext>(options =>
            options.UseInMemoryDatabase("ChatHistoryDb"));
        services.AddHttpContextAccessor();
        services.AddScoped<ConversationContext>();

        services.AddKeyedSingleton<AIAgent>("agent-with-persistence", (sp, _) =>
            sp.GetRequiredService<IChatClient>().AsAIAgent(new ChatClientAgentOptions
            {
                Name = "Agent Persistence",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant. Keep replies short and clear."
                },
                // Singleton agent: provider, ConversationId'yi istek anında scoped
                // ConversationContext'ten kendi içinde çözümlüyor.
                ChatHistoryProvider = new EfCoreChatHistoryProvider(sp)
            }));

        return services;
    }
}
