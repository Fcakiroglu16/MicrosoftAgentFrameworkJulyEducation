using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

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

        // Template'ler resources/list içinde gelmez; gerçek değerlerle URI oluşturup
        // ReadResourceAsync ile tek tek okumak gerekir.
        await ReadTemplateResourceAsync(client, "users://profiles/1");
        await ReadTemplateResourceAsync(client, "users://roles/admin/members");
    }

    private static async Task ReadTemplateResourceAsync(McpClient client, string uri)
    {
        System.Console.WriteLine($"\n--- Reading template resource: {uri} ---");

        var result = await client.ReadResourceAsync(uri);
        foreach (var content in result.Contents)
        {
            if (content is TextResourceContents text)
                System.Console.WriteLine(text.Text);
        }
    }
}