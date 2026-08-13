using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Orchestrations.Concurrent;

public static class ConcurrentTranslationWorkflow
{
    public static async Task RunAsync()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("=== Concurrent Orchestration: Çeviri (TR -> EN / DE / FR) ===");

        var result = await ExecuteAsync(
            "Merhaba dünya, bugün hava çok güzel!",
            (executorId, text) =>
            {
                System.Console.WriteLine();
                System.Console.Write($"{executorId}: ");
            },
            text => System.Console.Write(text));

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine("--- Final Aggregated Results ---");
        foreach (var message in result)
            System.Console.WriteLine($"{message.Role}: {message.Text}");
    }

    public static Task<List<ChatMessage>> ExecuteAsync(string textToTranslate)
    {
        return ExecuteAsync(textToTranslate, null, null);
    }

    private static async Task<List<ChatMessage>> ExecuteAsync(
        string textToTranslate,
        Action<string, string>? onExecutorChanged,
        Action<string>? onTextChunk)
    {
        var chatClient = CreateChatClient();

        string[] targetLanguages = ["English", "German", "French"];
        var translationAgents = targetLanguages.Select(lang => GetTranslationAgent(lang, chatClient));

        var workflow = AgentWorkflowBuilder.BuildConcurrent(translationAgents);

        var messages = new List<ChatMessage> { new(ChatRole.User, textToTranslate) };

        await using var run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(true));

        string? lastExecutorId = null;
        List<ChatMessage> result = new();
        await foreach (var evt in run.WatchStreamAsync())
            if (evt is AgentResponseUpdateEvent e)
            {
                // if (e.ExecutorId != lastExecutorId)
                // {
                //     lastExecutorId = e.ExecutorId;
                //     onExecutorChanged?.Invoke(e.ExecutorId, e.Update.Text ?? string.Empty);
                // }
                //
                // onTextChunk?.Invoke(e.Update.Text ?? string.Empty);
            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>()!;
                break;
            }

        return result;
    }

    private static IChatClient CreateChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Please set the OPEN_AI_KEY environment variable.");

        return new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient();
    }

    private static ChatClientAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient)
    {
        return new ChatClientAgent(chatClient,
            $"You are a translation assistant who only responds in {targetLanguage}. Respond to any " +
            $"input by outputting the name of the input language and then translating the input to {targetLanguage}.",
            $"{targetLanguage}_Agent");
    }
}