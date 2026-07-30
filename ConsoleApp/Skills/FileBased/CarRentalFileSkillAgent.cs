using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Skills.FileBased;


public static class CarRentalFileSkillAgent
{
    public static AIAgent CreateAgent()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException(
                         "Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

        var chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();

    
        var skillsPath = Path.Combine(AppContext.BaseDirectory, "Skills", "FileBased", "skills");

   
        var skillsProvider = new AgentSkillsProvider(
            skillPath: skillsPath,
            scriptRunner: null,
            fileOptions: null,
            options: new AgentSkillsProviderOptions
            {
                DisableLoadSkillApproval = true,
                DisableReadSkillResourceApproval = true,
                DisableRunSkillScriptApproval = true
            },
            loggerFactory: null);

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

        System.Console.WriteLine("--- Araç Kiralama Fiyat Teklifi (File-based Skill) ---");
        System.Console.WriteLine(response.Text);
    }
}
