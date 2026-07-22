using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console;


public static class  FunctionTool
{



    [Description("Get the current weather for a given location.")]
    static string GetWeather(
        [Description("The city or location to get the weather for.")] string location)
        => $"The weather in {location} is cloudy with a high of 15°C.";



   public static async Task Run()
   {
  var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
            ?? throw new InvalidOperationException(
                "Set the OPEN_AI_KEY environment variable before running this demo.");


 AIFunction weatherTool = AIFunctionFactory.Create(GetWeather);


     AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient()
            .AsAIAgent(
                instructions: "You are a helpful assistant. Use available tools when needed.",
                tools: [weatherTool]);

        const string prompt = "What is the weather like in Amsterdam?";
        System.Console.WriteLine($"Prompt  : {prompt}\n");

        AgentResponse response = await agent.RunAsync(prompt);

        System.Console.WriteLine($"Response: {response.Text}");
        System.Console.WriteLine("\n✓ Demo 1 completed.");









   }


}