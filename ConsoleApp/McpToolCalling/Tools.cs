using ModelContextProtocol.Client;

namespace App.Console.McpToolCalling;

public class Tools
{
    
    public static async Task ListToolAsync(McpClient client)
    {
        
        System.Console.Write("\n========== TOOLS ==========");

        if(client.ServerCapabilities.Tools == null)
        {
            System.Console.WriteLine("\nNo tools available on the server.");
            return;
        }
        
        
        var tools = await client.ListToolsAsync();
        System.Console.WriteLine($"Found {tools.Count} tool(s):");
        foreach (var tool in tools)
            System.Console.WriteLine($"  [{tool.Name}] {tool.Description}");
        
    }
}