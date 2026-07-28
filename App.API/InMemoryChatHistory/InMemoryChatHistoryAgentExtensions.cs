using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.API.InMemoryChatHistory;

public static class InMemoryChatHistoryAgentExtensions
{
    /// <summary>
    /// Registers the OpenAI chat client and an <see cref="AIAgent"/> configured with an
    /// <see cref="InMemoryChatHistoryProvider"/> so conversation history is kept in memory per session.
    /// </summary>
    public static IServiceCollection AddInMemoryChatHistoryAgent(this IServiceCollection services, string openAiKey)
    {
        var openAiClient = new OpenAIClient(openAiKey);

        services.AddChatClient(openAiClient.GetChatClient("gpt-4o-mini").AsIChatClient());
        services.AddSingleton<AIAgent>(_ =>
            openAiClient
                .GetChatClient("gpt-4o-mini")
                .AsIChatClient()
                .AsAIAgent(new ChatClientAgentOptions
                {
                    Name = "BasicLinearChat",
                    ChatOptions = new ChatOptions
                    {
                        Instructions = "You are a helpful assistant. Keep replies short and clear."
                    },
                    ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions()
                    {
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                        ChatReducer = new MessageCountingChatReducer(5)
#pragma warning restore MEAI001
                    })
                }));

        return services;
    }
}
