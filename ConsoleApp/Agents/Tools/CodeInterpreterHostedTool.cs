using System.Diagnostics.CodeAnalysis;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents.Tools;

/// <summary>
///     Ders 10 — Barındırılan Araçlar
///     Demo 2: Sorunları çözmek için korumalı bir ortamda kod yazıp çalıştırabilen,
///     barındırılan Code Interpreter aracını kullanan basit bir ajan.
/// </summary>
public static class CodeInterpreterHostedTool
{
    [Experimental("OPENAI001")]
    public static async Task RunAsync()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException(
                         "Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");
        System.Console.WriteLine("=== Demo 2: Kod Yorumlayıcı ===");
        System.Console.WriteLine();

        // Barındırılan kod yorumlayıcı aracı, OpenAI Responses API'sini gerektirir.
        AIAgent agent = new OpenAIClient(apiKey)
            .GetResponsesClient()
            .AsIChatClient("gpt-4o-mini")
            .AsAIAgent(
                "Kod yazıp çalıştırabilen yardımcı bir asistansın.",
                tools: [new HostedCodeInterpreterTool()]);

        System.Console.WriteLine("Soru: 1 ile 50 arasındaki sayıları topla.");
        System.Console.WriteLine();

        var response = await agent.RunAsync("1 ile 50 arasındaki sayıları topla. toplam sonucu dön");

        System.Console.WriteLine("Ajan Yanıtı:");
        System.Console.WriteLine(response);
        System.Console.WriteLine();
    }
}