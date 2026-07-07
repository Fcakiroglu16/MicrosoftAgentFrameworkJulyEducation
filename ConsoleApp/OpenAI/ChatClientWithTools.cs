using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.OpenAI
{
    internal class ChatClientWithTools
    {
        [Description("Belirtilen şehir için güncel hava durumunu döndürür.")]
        private static string GetWeather([Description("Hava durumu öğrenilecek şehir adı")] string city)
            => $"{city}: 22°C, güneşli";

        public async Task Run()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                System.Console.WriteLine("API key is missing.");
                return;
            }


            AIFunction weatherFunction = AIFunctionFactory.Create(
                GetWeather,
                name: "get_weather",
                description: "Belirtilen şehir için güncel hava durumunu döndürür.");


            ChatOptions options = new() { Tools = [weatherFunction] };


            IChatClient baseclient = new OpenAIClient(apiKey)
                .GetChatClient("gpt-4o-mini")
                .AsIChatClient();


            var client = baseclient.AsBuilder().UseFunctionInvocation().Build();


            //ResponseAsync method
            ChatResponse r1 = await client.GetResponseAsync("Ankara'da hava kaç derece?", options);


            System.Console.WriteLine(r1.Text);
        }
    }
}
