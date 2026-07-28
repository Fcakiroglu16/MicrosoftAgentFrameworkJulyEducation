using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Responses;

namespace App.API.ManagedStorageChatHistory;

public static class ManagedStorageChatHistoryAgentExtensions
{
    /// <summary>
    ///     Registers an <see cref="AIAgent" /> backed by the OpenAI Responses API. Conversation history is
    ///     stored and managed by the OpenAI service itself (service-managed storage); we only need to keep
    ///     track of the conversation id returned by the service and pass it back on subsequent requests.
    /// </summary>
    public static IServiceCollection AddManagedStorageChatHistoryAgent(this IServiceCollection services,
        OpenAIClient openAiClient)
    {
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
        services.AddKeyedSingleton<AIAgent>("agent-with-managed-storage", (_, _) =>
            openAiClient
                .GetResponsesClient()
                .AsAIAgent(
                    model: "gpt-4o-mini",
                    instructions: "You are a helpful assistant. Keep replies short and clear.",
                    name: "ServiceManagedChat"));
#pragma warning restore OPENAI001

        return services;
    }
}
