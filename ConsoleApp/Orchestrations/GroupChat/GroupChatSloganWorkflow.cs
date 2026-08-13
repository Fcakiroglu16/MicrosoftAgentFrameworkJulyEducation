using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Orchestrations.GroupChat;

/// <summary>
///     Group Chat Orchestration örneği:
///     Birden fazla ajan ORTAK bir sohbette konuşur. Kimin ne zaman konuşacağına
///     ortadaki bir "manager" (orkestratör) karar verir. Burada en basit strateji olan
///     sıralı (round-robin) seçim kullanılıyor:
///     Yazar -> Editör -> Pazarlama -> Hukuk -> Yazar -> ...
///     Senaryo: Bir kahve markası için slogan üretme toplantısı.
///     SloganYazarAgent : slogan önerir
///     EditorAgent      : dil ve etki açısından eleştirir
///     PazarlamaAgent   : hedef kitleye uygunluğunu değerlendirir
///     HukukAgent       : yasal/marka riski var mı diye bakar
///     Ajanlar tüm sohbet geçmişini gördüğü için her turda slogan biraz daha iyileşir.
/// </summary>
public static class GroupChatSloganWorkflow
{
    public static async Task RunAsync()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("=== Group Chat Orchestration: Slogan Toplantısı (4 Ajan) ===");

        const string task = "Çevre dostu, geri dönüştürülebilir bardak kullanan bir kahve markası için slogan yaz.";

        System.Console.WriteLine();
        System.Console.WriteLine($"Görev: {task}");

        var conversation = await ExecuteAsync(task);

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine("--- Final Sohbet ---");
        foreach (var message in conversation)
            System.Console.WriteLine($"{message.AuthorName ?? message.Role.ToString()}: {message.Text}");
    }

    private static async Task<List<ChatMessage>> ExecuteAsync(string task)
    {
        var chatClient = CreateChatClient();

        ChatClientAgent writer = new(
            chatClient,
            """
            Sen yaratıcı bir slogan yazarısın.
            - Kısa, akılda kalıcı ve Türkçe slogan öner.
            - Her turda TEK bir slogan öner.
            - Diğer ajanlardan (editör, pazarlama, hukuk) gelen geri bildirimleri dikkate alıp sloganı geliştir.
            """,
            "SloganYazarAgent",
            "Slogan üreten yaratıcı yazar");

        ChatClientAgent editor = new(
            chatClient,
            """
            Sen bir pazarlama editörüsün.
            - Önerilen sloganı netlik, etki ve marka uyumu açısından değerlendir.
            - En fazla 2 cümleyle yapıcı geri bildirim ver.
            - Slogan yeterince iyiyse "ONAYLANDI" yazarak bitir.
            """,
            "EditorAgent",
            "Slogandaki eksikleri değerlendiren editör");

        ChatClientAgent marketer = new(
            chatClient,
            """
            Sen bir pazarlama uzmanısın.
            - Sloganın hedef kitleye (şehirde yaşayan, çevreye duyarlı genç profesyoneller) uygunluğunu değerlendir.
            - En fazla 2 cümleyle yorum yap ve gerekiyorsa somut bir öneri ver.
            """,
            "PazarlamaAgent",
            "Sloganı hedef kitle açısından değerlendiren pazarlama uzmanı");

        ChatClientAgent legal = new(
            chatClient,
            """
            Sen bir marka ve hukuk danışmanısın.
            - Sloganda yanıltıcı çevre iddiası veya başka bir markayı çağrıştıran ifade var mı kontrol et.
            - En fazla 2 cümleyle risk belirt; risk yoksa "HUKUKEN UYGUN" yaz.
            """,
            "HukukAgent",
            "Sloganı yasal ve marka riski açısından kontrol eden danışman");

        // Round-robin manager: ajanlar eklenme sırasına göre sırayla konuşur.
        // 4 ajan x 2 tur = 8 konuşma.
        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
                new RoundRobinGroupChatManager(agents)
                {
                    MaximumIterationCount = 8
                })
            .AddParticipants(writer, editor, marketer, legal)
            .Build();

        var messages = new List<ChatMessage> { new(ChatRole.User, task) };

        await using var run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(true));

        string? lastExecutorId = null;
        List<ChatMessage> conversation = new();

        await foreach (var evt in run.WatchStreamAsync())
            if (evt is AgentResponseUpdateEvent e)
            {
                // Hangi ajanın sırası geldiğini görmek group chat akışını anlamayı kolaylaştırır.
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    System.Console.WriteLine();
                    System.Console.Write($"[{e.ExecutorId}]: ");
                }

                System.Console.Write(e.Update.Text);
            }
            else if (evt is WorkflowOutputEvent output)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("-----------------------------");
                conversation = output.As<List<ChatMessage>>()!;
                break;
            }

        return conversation;
    }

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