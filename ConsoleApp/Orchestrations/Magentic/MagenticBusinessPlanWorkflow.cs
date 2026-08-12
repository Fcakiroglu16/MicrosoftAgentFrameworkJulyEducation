#pragma warning disable MAAIW001 // Magentic types are experimental

using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Specialized.Magentic;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Orchestrations.Magentic;

/// <summary>
/// Magentic Orchestration örneği:
/// Group Chat'e benzer ama ortadaki "manager" çok daha akıllıdır.
/// Manager önce bir PLAN yapar, sonra her adımda hangi ajanın konuşacağına
/// duruma göre karar verir, ilerlemeyi takip eder ve tıkanırsa planı yeniler.
/// Bu yüzden çözüm yolunun baştan belli olmadığı, açık uçlu görevler için uygundur.
///
/// Senaryo: "İstanbul'da küçük bir kahve dükkanı açmak istiyorum" analizi.
///   ArastirmaAgent : maliyet kalemlerini ve piyasa bilgisini toplar
///   HesaplamaAgent : toplama/başabaş noktası gibi sayısal hesapları yapar
///   MagenticManager: planı kurar, sırayı yönetir, sonucu özetler
/// </summary>
public static class MagenticBusinessPlanWorkflow
{
    private const string TaskPrompt =
        "İstanbul'da 40 m2'lik küçük bir kahve dükkanı açmak istiyorum. " +
        "Aylık sabit giderleri (kira, personel, faturalar) makul tahminlerle listele, " +
        "bir bardak kahvenin maliyetini ve satış fiyatını varsay, " +
        "sonra ayda kaç bardak kahve satarsam başabaş noktasına ulaşırım hesapla. " +
        "Sonucu kısa bir tablo ve tek paragraflık öneriyle özetle.";

    public static async Task RunAsync()
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        WriteBlock("MAGENTIC ORCHESTRATION: İŞ PLANI ANALİZİ", TaskPrompt, ConsoleColor.White);

        var workflow = BuildWorkflow();

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
            workflow,
            new List<ChatMessage> { new(ChatRole.User, TaskPrompt) });

        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        // Ajan cevapları parça parça (streaming) gelir. Konsolun karışmaması için
        // parçaları biriktirip cevap tamamlandığında tek blok halinde yazdırıyoruz.
        string? currentResponseId = null;
        string? currentAgentName = null;
        var buffer = new StringBuilder();
        WorkflowOutputEvent? finalOutput = null;

        void FlushAgentResponse()
        {
            if (currentAgentName is not null && buffer.Length > 0)
                WriteBlock($"AJAN: {currentAgentName}", buffer.ToString().Trim(), ConsoleColor.Cyan);

            buffer.Clear();
            currentAgentName = null;
            currentResponseId = null;
        }

        await foreach (WorkflowEvent workflowEvent in run.WatchStreamAsync())
        {
            switch (workflowEvent)
            {
                case AgentResponseUpdateEvent updateEvent:
                    // Aynı cevabın parçalarını gruplamak için ResponseId kullanılır.
                    string responseId = updateEvent.Update.ResponseId
                        ?? updateEvent.Update.MessageId
                        ?? updateEvent.ExecutorId;

                    if (!string.Equals(responseId, currentResponseId, StringComparison.Ordinal))
                    {
                        FlushAgentResponse();
                        currentResponseId = responseId;
                        currentAgentName = updateEvent.ExecutorId;
                    }

                    buffer.Append(updateEvent.Update.Text);
                    break;

                case MagenticPlanCreatedEvent planCreated:
                    // Manager'ın en başta kurduğu plan.
                    FlushAgentResponse();
                    WriteBlock("MANAGER > İLK PLAN", planCreated.FullTaskLedger.Text, ConsoleColor.Yellow);
                    break;

                case MagenticReplannedEvent replanned:
                    // İlerleme olmazsa manager planı yeniden kurar.
                    FlushAgentResponse();
                    WriteBlock("MANAGER > YENİ PLAN", replanned.FullTaskLedger.Text, ConsoleColor.Yellow);
                    break;

                case MagenticProgressLedgerUpdatedEvent progressUpdated:
                    // Manager her turda "hedefe ulaşıldı mı, sıradaki kim" diye karar verir.
                    FlushAgentResponse();
                    MagenticProgressLedger ledger = progressUpdated.ProgressLedger;
                    WriteBlock(
                        "MANAGER > İLERLEME DURUMU",
                        $"Tamamlandı mı : {ledger.IsRequestSatisfied}{Environment.NewLine}" +
                        $"Döngüde mi  : {ledger.IsInLoop}{Environment.NewLine}" +
                        $"İlerliyor mu: {ledger.IsProgressBeingMade}{Environment.NewLine}" +
                        $"Sıradaki    : {ledger.NextSpeaker}{Environment.NewLine}" +
                        $"Talimat     : {ledger.InstructionOrQuestion}",
                        ConsoleColor.DarkGray);
                    break;

                case WorkflowOutputEvent outputEvent when outputEvent.Is<List<ChatMessage>>():
                    FlushAgentResponse();
                    finalOutput = outputEvent;
                    break;

                case WorkflowErrorEvent workflowError:
                    FlushAgentResponse();
                    WriteBlock("HATA", workflowError.Exception?.ToString() ?? "Bilinmeyen workflow hatası.", ConsoleColor.Red);
                    break;

                case ExecutorFailedEvent executorFailed:
                    FlushAgentResponse();
                    WriteBlock(
                        $"HATA: {executorFailed.ExecutorId}",
                        executorFailed.Data?.ToString() ?? "bilinmeyen hata",
                        ConsoleColor.Red);
                    break;
            }
        }

        FlushAgentResponse();

        if (finalOutput?.As<List<ChatMessage>>() is { } transcript)
        {
            System.Console.WriteLine();
            WriteBlock("FİNAL SONUÇ", string.Empty, ConsoleColor.Green);

            foreach (ChatMessage message in transcript)
                WriteBlock(message.AuthorName ?? message.Role.ToString(), message.Text?.Trim() ?? string.Empty, ConsoleColor.Green);
        }
    }

    /// <summary>Bir başlık ve gövdeyi çerçeveli, renkli ve okunaklı bir blok olarak yazar.</summary>
    private static void WriteBlock(string title, string body, ConsoleColor color)
    {
        const int width = 90;
        var previousColor = System.Console.ForegroundColor;

        System.Console.WriteLine();
        System.Console.ForegroundColor = color;
        System.Console.WriteLine(new string('─', width));
        System.Console.WriteLine($"│ {title}");
        System.Console.WriteLine(new string('─', width));
        System.Console.ForegroundColor = previousColor;

        if (!string.IsNullOrWhiteSpace(body))
            System.Console.WriteLine(body);
    }

    private static Workflow BuildWorkflow()
    {
        var chatClient = CreateChatClient();

        AIAgent researcher = new ChatClientAgent(
            chatClient,
            """
            Sen bir pazar araştırmacısısın.
            - İstenen konuda makul, gerçekçi tahmini bilgileri ve maliyet kalemlerini topla.
            - Hesap yapma, sadece bilgi ve varsayımları net şekilde listele.
            - Türkçe ve maddeler halinde yaz.
            """,
            "ArastirmaAgent",
            "Maliyet kalemlerini ve piyasa bilgilerini toplayan araştırmacı");

        AIAgent calculator = new ChatClientAgent(
            chatClient,
            """
            Sen bir finansal analistsin.
            - Araştırmacının verdiği rakamları kullanarak hesap yap.
            - Toplam gider, birim kâr ve başabaş noktası hesabını adım adım göster.
            - Türkçe yaz ve sonucu küçük bir tablo ile özetle.
            """,
            "HesaplamaAgent",
            "Sayısal hesaplamaları ve başabaş analizini yapan analist");

        AIAgent manager = new ChatClientAgent(
            chatClient,
            "Ekibi koordine ederek karmaşık görevi verimli şekilde tamamlarsın.",
            "MagenticManager",
            "Planı kuran ve ajanları yöneten orkestratör");

        return new MagenticWorkflowBuilder(manager)
            // Ekibe katılan uzman ajanlar. Manager sırayı bunlar arasından seçer.
            .AddParticipants([researcher, calculator])
            // Workflow'un adı; loglarda/izlemede bu akışı tanımak için kullanılır.
            .WithName("Magentic Business Plan Workflow")
            // Workflow'un ne yaptığını anlatan açıklama (dokümantasyon/izleme amaçlı).
            .WithDescription("Bir kahve dükkanı için araştırma ve başabaş analizini koordine eder.")
            // false: plan onaya sunulmaz, manager planı yapıp doğrudan uygular.
            // true olsaydı plan önce insana gösterilip onay beklenirdi (£n-in-the-loop).
            .RequirePlanSignoff(false)
            // Manager'ın koordinasyon döngüsünde en fazla 10 tur çalışmasına izin ver.
            // Sonsuz döngüye girip maliyet çıkarmasını engelleyen güvenlik sınırı.
            .WithMaxRounds(10)
            // Üst üste 3 tur ilerleme olmazsa (takılma) manager planı yeniden kurar.
            .WithMaxStalls(3)
            // Plan en fazla 2 kez sıfırlanabilir; sonrasında workflow sonlanır.
            .WithMaxResets(2)
            .Build();
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
