using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Skills.Inline;

public static class CarRentalInlineSkillAgent
{
    public static AIAgent CreateAgent()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException(
                         "Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

        var chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();

      
        var pricingSkill = new AgentInlineSkill(
                name: "car-pricing-skill",
                description: "Araç kiralamaları için toplam ücreti hesaplar",
                instructions: """
                    İstenen araç kategorisi için fiyatlandırmayı belirlemek için bu skill'i kullan.
                    1. Günlük ücreti öğrenmek için pricing-table kaynağını kontrol et.
                    2. Günlük ücreti istenen gün sayısıyla çarp.
                    """)
            .AddResource(
                "pricing-table",
                """
                | Kategori   | Günlük Ücret |
                |------------|--------------|
                | Ekonomik   | 30$          |
                | Sedan      | 45$          |
                | SUV        | 65$          |
                """,
                "Araç kategorilerine göre günlük kiralama ücretlerini içeren tablo");
        
        var skillsProvider = new AgentSkillsProvider(pricingSkill);

        var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "AracKiralamaAsistani",
            Description = "Araç kiralama fiyatlarını hesaplayan bir yapay zeka asistanı.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               Sen bir araç kiralama fiyatlandırma asistanısın.
                               Kullanıcı bir araç kategorisi ve kiralama süresi belirttiğinde,
                               car-pricing-skill'i kullanarak toplam maliyeti hesapla ve
                               kullanıcıya net, kısa bir yanıt ver.
                               """
            },
            AIContextProviders = [skillsProvider]
        });

        return agent;
    }

    public static async Task Run()
    {
        var agent = CreateAgent();

        var response = await agent.RunAsync(
            "3 gün boyunca bir SUV kiralamak istiyorum, toplam ücret ne kadar tutar?");

        System.Console.WriteLine("--- Araç Kiralama Fiyat Teklifi ---");
        System.Console.WriteLine(response.Text);
    }
}
