using System.ClientModel.Primitives;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Azure;

internal class ChatClient
{
    public async Task Run()
    {
        const string deploymentName = "gpt-5-mini";
        const string endpoint = "https://education-test-resource.services.ai.azure.com/openai/v1";
        BearerTokenPolicy tokenPolicy = new(
            new DefaultAzureCredential(),
            "https://ai.azure.com/.default");

#pragma warning disable OPENAI001 // Experimental: AuthenticationPolicy-based
        var client = new OpenAIClient(
                tokenPolicy,
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
            .GetChatClient(deploymentName)
            .AsIChatClient();
    }
}