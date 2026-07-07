using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;

namespace App.Console.OpenAI
{
    public enum Sentiment
    {
        Positive,
        Negative,
        Neutral
    }

    internal class ChatClientWithOutput
    {
        public async Task Run()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                System.Console.WriteLine("API key is missing.");
                return;
            }


            IChatClient client = new OpenAIClient(apiKey)
                .GetChatClient("gpt-4o-mini")
                .AsIChatClient();

            //ResponseAsync method
            var r1 = await client.GetResponseAsync<Sentiment>("bu ürün çok kullanışlı.");

            Sentiment sentiment = r1.Result;

            System.Console.WriteLine(sentiment);
        }
    }
}
