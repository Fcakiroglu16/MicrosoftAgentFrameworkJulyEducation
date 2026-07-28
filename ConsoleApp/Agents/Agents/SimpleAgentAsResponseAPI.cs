using System.Diagnostics.CodeAnalysis;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents.Agents;

public static class SimpleAgentAsResponseAPIAsResponseAPI
{
    [Experimental("OPENAI001")]
    public static async Task Run()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException(
                         "Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

        // Microsoft.Extensions.AI.OpenAI üzerinden IChatClient oluşturma
        var chatClient = new OpenAIClient(apiKey).GetResponsesClient().AsIChatClient("gpt-4o");

        // Sistem talimatlarıyla ChatClientAgent oluşturma
        var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "ProjeKoordinatoru",
            Description =
                "Karmaşık iş ve projeleri gerçek dünya koşullarına uygun, net ve uygulanabilir adımlara bölen bir yapay zeka asistanı.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               Sen deneyimli bir proje koordinatörüsün.
                               Kullanıcının verdiği her işi gerçek dünya koşullarına uygun,
                               net, ölçülebilir ve sıralı adımlara böl.
                               Her adımda sorumlu rolü, tahmini süreyi,
                               ihtiyaç duyulan kaynakları ve olası riskleri belirt.
                               Teknik jargon kullanma; tüm paydaşların anlayabileceği
                               açık ve profesyonel bir dille yaz.
                               """
            }
        });

        // // Agent Framework üzerinden isteği çalıştırma
        // var response = await agent.RunAsync(
        //     "Şirketin e-ticaret sitesine yeni bir ödeme entegrasyonu eklenmesi gerekiyor. Proje ekibini organize et, görevleri ve sorumluları belirle, zaman çizelgesi oluştur ve yöneticiye kısa bir özet rapor hazırla.");
        //
        // System.Console.WriteLine("--- Proje Koordinasyon Planı ---");
        // System.Console.WriteLine(response.Text);
    }
}