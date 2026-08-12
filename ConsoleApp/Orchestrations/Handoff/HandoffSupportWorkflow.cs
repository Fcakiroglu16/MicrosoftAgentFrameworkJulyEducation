using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Orchestrations.Handoff;

/// <summary>
/// Handoff Orchestration örneği:
/// Bir "Karşılama (Triage)" ajanı gelen soruyu okur ve konuya göre
/// ilgili uzman ajana sohbeti DEVREDER (handoff). Devralan ajan konuşmanın
/// tamamını görür ve cevabı o verir.
///
/// Senaryo: Basit bir e-ticaret müşteri hizmetleri hattı.
///   TriageAgent  -> KargoAgent | IadeAgent
///   KargoAgent   -> TriageAgent
///   IadeAgent    -> TriageAgent
/// </summary>
public static class HandoffSupportWorkflow
{
    private static readonly string[] DemoQuestions =
    [
        "Siparişim nerede, kargoya verildi mi?",
        "Aldığım ürünü iade etmek istiyorum, ne yapmalıyım?"
    ];

    public static async Task RunAsync()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("=== Handoff Orchestration: Müşteri Hizmetleri (Triage -> Kargo / İade) ===");

        var workflow = BuildWorkflow();

        // Konuşma geçmişi: her turda büyür, böylece ajanlar önceki mesajları da görür.
        List<ChatMessage> messages = new();

        foreach (var question in DemoQuestions)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Müşteri: {question}");

            messages.Add(new ChatMessage(ChatRole.User, question));

            var newMessages = await RunTurnAsync(workflow, messages);

            // Bu turda üretilen yeni mesajları geçmişe ekle.
           messages.AddRange(newMessages.Skip(messages.Count));
        }

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine("--- Konuşma Özeti ---");
        foreach (var message in messages)
            System.Console.WriteLine($"{message.Role}: {message.Text}");
    }

    private static Workflow BuildWorkflow()
    {
        var chatClient = CreateChatClient();

        var triageAgent = CreateAgent(
            chatClient,
            name: "TriageAgent",
            description: "Gelen müşteri sorusunu doğru uzmana yönlendirir.",
            instructions: """
                          Sen bir müşteri hizmetleri karşılama asistanısın.
                          - Müşterinin sorusunu oku ve konusunu belirle.
                          - Kargo/teslimat ile ilgiliyse KargoAgent'a devret.
                          - İade/iptal/para iadesi ile ilgiliyse IadeAgent'a devret.
                          - Soruyu KENDİN cevaplama, HER ZAMAN uygun ajana devret.
                          - Devretmeden önce tek cümlelik kısa bir bilgilendirme yaz.
                          """);

        var kargoAgent = CreateAgent(
            chatClient,
            name: "KargoAgent",
            description: "Kargo ve teslimat sorularını yanıtlar.",
            instructions: """
                          Sen bir kargo ve teslimat uzmanısın.
                          - Sadece kargo, teslimat süresi ve takip numarası konularında yardımcı ol.
                          - Kısa, net ve Türkçe cevap ver.
                          - Konu senin alanın dışındaysa TriageAgent'a geri devret.
                          """);

        var iadeAgent = CreateAgent(
            chatClient,
            name: "IadeAgent",
            description: "İade ve iptal sorularını yanıtlar.",
            instructions: """
                          Sen bir iade ve iptal uzmanısın.
                          - Sadece iade koşulları, iade adımları ve para iadesi konularında yardımcı ol.
                          - Kısa, net ve Türkçe cevap ver.
                          - Konu senin alanın dışındaysa TriageAgent'a geri devret.
                          """);

        // Handoff kuralları: kimin kime devredebileceğini burada tanımlıyoruz.
        return AgentWorkflowBuilder.CreateHandoffBuilderWith(triageAgent)
            .WithHandoffs(triageAgent, [kargoAgent, iadeAgent])
            .WithHandoffs([kargoAgent, iadeAgent], triageAgent)
            .Build();
    }

    private static async Task<List<ChatMessage>> RunTurnAsync(Workflow workflow, List<ChatMessage> messages)
    {
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        string? lastExecutorId = null;
        List<ChatMessage> newMessages = new();

        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent e)
            {
                // Hangi ajanın konuştuğunu görmek handoff'u anlamak için önemli.
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    System.Console.WriteLine();
                    System.Console.Write($"{e.ExecutorId}: ");
                }
                
                System.Console.Write(e.Update.Text);
            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                newMessages = outputEvt.As<List<ChatMessage>>()!;
                break;
            }
        }

        return newMessages;
    }

    private static ChatClientAgent CreateAgent(IChatClient chatClient, string name, string description, string instructions) =>
        new(chatClient, instructions, name, description);

    private static IChatClient CreateChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Please set the OPEN_AI_KEY environment variable.");

        return new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient();
    }
}
