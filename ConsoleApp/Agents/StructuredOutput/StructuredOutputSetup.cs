using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents.StructuredOutput;

public static class StructuredOutputSetup
{
    public static IChatClient CreateChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Please set the OPEN_AI_KEY environment variable.");

        return new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient();
    }


    public static ChatClientAgent CreateStructuredAgent<T>(
        IChatClient chatClient,
        string agentName,
        string instructions,
        string schemaDescription)
    {
        // Create JSON schema from the type
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(T));

        // Configure chat options to use structured output
        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema,
                typeof(T).Name,
                schemaDescription)
        };

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = agentName,
            Description = instructions,
            ChatOptions = chatOptions
        });
    }

    /// <summary>
    ///     Pretty print JSON object
    /// </summary>
    public static void PrintJson<T>(T obj) where T : class
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        System.Console.WriteLine(json);
    }
}