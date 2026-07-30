using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace App.Console.McpToolCalling;

public abstract class Prompts
{
    public static async Task ListPromptsAsync(McpClient client)
    {
        System.Console.WriteLine("\n========== PROMPTS ==========");

        var prompts = await client.ListPromptsAsync();
        System.Console.WriteLine($"Found {prompts.Count} prompt(s):");
        foreach (var prompt in prompts)
            System.Console.WriteLine($"  [{prompt.Name}] {prompt.Description}");

      
    }

    /// <summary>
    /// Sunucudan adı verilen prompt'u (gerekiyorsa argümanlarla birlikte) çeker
    /// ve hazır ChatMessage listesine dönüştürür.
    /// </summary>
    public static async Task<IList<ChatMessage>> GetPromptAsync(
        McpClient client,
        string name,
        IReadOnlyDictionary<string, object?>? arguments = null)
    {
        System.Console.WriteLine($"\n--- Fetching prompt: {name} ---");

        var result = await client.GetPromptAsync(name, arguments);
        var messages = result.ToChatMessages();

        foreach (var message in messages)
            System.Console.WriteLine($"  [{message.Role}] {message.Text}");

        return messages;
    }
}