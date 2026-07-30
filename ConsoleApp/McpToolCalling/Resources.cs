using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Linq;

namespace App.Console.McpToolCalling;

public class Resources
{
    public static async Task ListStaticResourcesAsync(McpClient client)
    {
        System.Console.WriteLine("\n========== RESOURCES ==========");

        var resources = await client.ListResourcesAsync();
        System.Console.WriteLine($"Found {resources.Count} resource(s):");
        foreach (var resource in resources)
            System.Console.WriteLine($"  [{resource.Name}] {resource.Uri}");
    }

    public static async Task ListAndReadTemplateResourcesAsync(McpClient client)
    {
        System.Console.WriteLine("\n========== RESOURCE TEMPLATES ==========");

        var templates = await client.ListResourceTemplatesAsync();
        System.Console.WriteLine($"Found {templates.Count} resource template(s):");
        foreach (var template in templates)
            System.Console.WriteLine($"  [{template.Name}] {template.UriTemplate}");

        // Templates don't show up in resources/list; you need to build the actual URI
        // with real values and read them one by one via ReadResourceAsync.
        await ReadTemplateResourceAsync(client, "users://profiles/1");
        await ReadTemplateResourceAsync(client, "users://roles/admin/members");
    }

    public static async Task<string> ReadTemplateResourceAsync(McpClient client, string uri)
    {
        System.Console.WriteLine($"\n--- Reading template resource: {uri} ---");

        var result = await client.ReadResourceAsync(uri);
        var text = string.Join(
            Environment.NewLine,
            result.Contents.OfType<TextResourceContents>().Select(c => c.Text));

        System.Console.WriteLine(text);
        return text;
    }
}