using System.ClientModel.Primitives;
using App.Console.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;

Console.WriteLine("Microsoft Extensions AI Chat Client");

var chatClient = new ChatClientWithOutput();

await chatClient.Run();

