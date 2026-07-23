using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents.Tools;


public static class ToolApproval
{
    // ── Function Tool ────────────────────────────────────────────────────────
    [Description("Belirtilen konum için güncel hava durumunu getirir.")]
    static string GetWeather(
        [Description("Hava durumu sorgulanacak şehir veya konum.")] string location)
        => $"{location} için hava durumu: bulutlu, en yüksek 15°C.";

    public static async Task RunAsync()
    {
        System.Console.WriteLine("╔═══════════════════════════════════════════════╗");
        System.Console.WriteLine("║  DEMO 1 — Araç Onayı (Onay yolu)             ║");
        System.Console.WriteLine("╚═══════════════════════════════════════════════╝\n");

        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
            ?? throw new InvalidOperationException(
                "Bu demoyu çalıştırmadan önce OPEN_AI_KEY ortam değişkenini ayarlayın.");

     
        AIFunction weatherTool = AIFunctionFactory.Create(GetWeather);
        AIFunction approvalRequiredWeatherTool = new ApprovalRequiredAIFunction(weatherTool);

        AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient()
            .AsAIAgent(
                instructions: "Yardımcı bir asistansın. Gerektiğinde mevcut araçları kullan.",
                tools: [approvalRequiredWeatherTool]);

     
      

        const string prompt = "Ankara'da hava nasıl?";
        System.Console.WriteLine($"Prompt  : {prompt}\n");

        // ── 1. Tur ───────────────────────────────────────────────────────────
        // Ajan GetWeather'ı çağırması gerektiğini fark eder ancak henüz çalıştıramaz.
        // Yanıt metin yerine FunctionApprovalRequestContent içerecektir.
        AgentResponse response = await agent.RunAsync(prompt);

        // Yanıt mesajlarından bekleyen tüm onay isteklerini çıkar.
        var approvalRequests = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<ToolApprovalRequestContent>()
            .ToList();

        if (approvalRequests.Count == 0)
        {
            // Onay istenmedi — ajan doğrudan yanıtladı (burada olmamalı).
            System.Console.WriteLine($"Yanıt: {response.Text}");
            return;
        }

        // ── Bekleyen istekler kullanıcıya gösteriliyor ────────────────────────
        System.Console.WriteLine($"Ajan {approvalRequests.Count} araç çağrısı için onay bekliyor:\n");
        foreach (var req in approvalRequests)
        {
            var functionCall = (FunctionCallContent)req.ToolCall;
            var arguments = functionCall.Arguments is { Count: > 0 }
                ? string.Join(", ", functionCall.Arguments.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "(yok)";
            System.Console.WriteLine($"  Araç      : {functionCall.Name}");
            System.Console.WriteLine($"  Argümanlar : {arguments}");
        }

        System.Console.Write("\nOnaylıyor musunuz? (e/h): ");
        bool approved = true;//System.Console.ReadLine()?.Trim().ToLower() == "e";

        // ── 2. Tur ───────────────────────────────────────────────────────────
        // Tüm onay/ret yanıtlarını bir araya getiren tek bir ChatMessage oluştur.
        // Ajanın tam konuşma bağlamına sahip olması için aynı oturumu ilet.
        var approvalContents = approvalRequests
            .Select(req => req.CreateResponse(approved))
            .ToArray<AIContent>();

        
        var approvalMessage = new ChatMessage(ChatRole.User, approvalContents);
        AgentResponse finalResponse = await agent.RunAsync(approvalMessage);

        System.Console.WriteLine(approved
            ? $"\nOnaylandı — Yanıt: {finalResponse.Text}"
            : $"\nReddedildi — Yanıt: {finalResponse.Text}");

        System.Console.WriteLine("\n✓ Demo 1 tamamlandı.");
    }
}
