using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace App.Console.Agents.MessageTypes;

/// <summary>
///     DEMO 1 — Basic Image Analysis
///     Demonstrates sending a local image file to an agent using DataContent.
/// </summary>
public static class MultiModelContentAgent
{
    public static async Task RunAsync(AIAgent agent)
    {
        var imagePath = Path.Combine(AppContext.BaseDirectory, "bird.jpg");
        const string imageUrl = "https://images.pexels.com/photos/326900/pexels-photo-326900.jpeg?w=640";

        System.Console.WriteLine($"Local image : {imagePath}");
        System.Console.WriteLine($"Remote image: {imageUrl}\n");

        var imageBytes = await File.ReadAllBytesAsync(imagePath);

        ChatMessage message = new(ChatRole.User, [
            new TextContent("Describe both images in detail. What do you see in each one?"),
            new DataContent(imageBytes, "image/jpeg"),
            new UriContent(imageUrl, "image/jpeg")
        ]);

        System.Console.WriteLine("Agent response:\n");
        var response = await agent.RunAsync(message);
        System.Console.WriteLine(response);

        System.Console.WriteLine("\n\n✓ Demo completed successfully!");
    }
}