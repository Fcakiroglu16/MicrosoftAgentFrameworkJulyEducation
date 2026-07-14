using Microsoft.Extensions.AI;
using OpenAI;

namespace App.API.Extensions;

internal static class AiServiceCollectionExtensions
{
    /// <summary>
    /// Registers the OpenAI chat clients (default, "fast", "smart") and the embedding generator.
    /// </summary>
    internal static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException("OPEN_AI_KEY ortam değişkenini ayarlayın.");

        var openAiClient = new OpenAIClient(apiKey);
        var openAiChatClient = openAiClient.GetChatClient("gpt-4o");

        services.AddChatClient(
                openAiChatClient.AsIChatClient())
            .UseFunctionInvocation().UseLogging().UseOpenTelemetry(sourceName: "chat-client-source");

        services.AddSingleton(
            openAiClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());

        services.AddKeyedChatClient(
                "fast",
                openAiClient.GetChatClient("gpt-4o-mini").AsIChatClient())
            .UseFunctionInvocation();

        services.AddKeyedChatClient(
                "smart",
                openAiClient.GetChatClient("gpt-4o").AsIChatClient())
            .UseFunctionInvocation();

        return services;
    }
}
