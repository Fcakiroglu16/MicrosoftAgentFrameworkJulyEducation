using Azure.Identity;
using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Azure
{
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
            IChatClient client = new OpenAIClient(
                    authenticationPolicy: tokenPolicy,
                    options: new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
                .GetChatClient(deploymentName)
                .AsIChatClient();
        }
    }
}
