using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.OpenAI
{
    internal class ChatClient
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


            IChatClient client = new OpenAIClient(apiKey)
                .GetChatClient("gpt-4o-mini")
                .AsIChatClient();

            //ResponseAsync method
            //ChatResponse r1 = await client.GetResponseAsync("Bilgisayarlarda RAM ne anlama gelir?");

            //System.Console.WriteLine(r1.Text);

            //GetStreamingResponseAsync method
            //await foreach (ChatResponseUpdate update in
            //               client.GetStreamingResponseAsync("Bir hash tablosunun ne olduğunu 3 cümlede açıklayın."))
            //{
            //    System.Console.Write(update.Text);
            //}


            //Chat History
            //string[] questions =
            //[
            //    "Bağlı liste (linked list) nedir?",
            //    "Bir diziden (array) farkı nedir?",
            //    "Ben sana linked list nedir diye sordum mu?"
            //];

            //// System.Prompt
            //// User.Prompt
            //// Assistant.Prompt
            //// User.Prompt
            //// Assistant.Prompt


            //var history = new List<ChatMessage>();

            //history.Add(new ChatMessage(ChatRole.System, "You are a helpful assistant."));

            //foreach (var question in questions)
            //{
            //    history.Add(new ChatMessage(ChatRole.User, question));

            //    ChatResponse response = await client.GetResponseAsync(history);
            //    history.Add(new ChatMessage(ChatRole.Assistant, response.Text));
            //    System.Console.WriteLine(response.Text);
            //    System.Console.WriteLine("------");
            //}
        }
    }
}
