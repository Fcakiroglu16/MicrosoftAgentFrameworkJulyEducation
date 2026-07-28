using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace App.API.InMemoryChatHistory;

public static class InMemoryChatHistoryAgentExtensions
{
    /// <summary>
    ///     Registers an <see cref="AIAgent" /> using the <see cref="IChatClient" /> resolved from the DI
    ///     container, configured with an <see cref="InMemoryChatHistoryProvider" /> so conversation history
    ///     is kept in memory per session.
    /// </summary>
    public static IServiceCollection AddInMemoryChatHistoryAgent(this IServiceCollection services)
    {
        services.AddSingleton<AIAgent>(sp =>
            sp.GetRequiredService<IChatClient>()
                .AsAIAgent(new ChatClientAgentOptions
                {
                    Name = "BasicLinearChat",
                    ChatOptions = new ChatOptions
                    {
                        Instructions = "You are a helpful assistant. Keep replies short and clear."
                    },
                    ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
                    {
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                        ChatReducer = new MessageCountingChatReducer(5)
#pragma warning restore MEAI001
                    })
                }));

        return services;
    }
}