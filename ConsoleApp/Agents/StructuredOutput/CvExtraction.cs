using System.Text.Json;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents.StructuredOutput;

/// <summary>
///     Extracts structured CV information from a PDF document.
/// </summary>
internal static class CvExtraction
{
    private const string CvFileName = "fatih_cakiroglu_cv.pdf";

    /// <summary>
    ///     Sends the CV PDF to the agent and prints its structured content.
    /// </summary>
    public static async Task RunAsync()
    {
        System.Console.WriteLine("╔═══════════════════════════════════════════════╗");
        System.Console.WriteLine("║  DEMO 6 — CV Information Extraction           ║");
        System.Console.WriteLine("╚═══════════════════════════════════════════════╝\n");

        var cvPath = Path.Combine(AppContext.BaseDirectory, CvFileName);
        if (!File.Exists(cvPath)) throw new FileNotFoundException($"CV file was not found at '{cvPath}'.", cvPath);


        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException(
                         "Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

        // Microsoft.Extensions.AI.OpenAI üzerinden IChatClient oluşturma
        var chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();


        var agent = StructuredOutputSetup.CreateStructuredAgent<CvInfo>(
            chatClient,
            "CvExtractor",
            "Extract only information explicitly present in the supplied CV. " +
            "Return the candidate's name, email, phone number, skills, work experience, and education. " +
            "Do not infer or invent missing information.",
            "CV information containing name, email, phone, skills, experience, and education");

        var pdfContent = await DataContent.LoadFromAsync(cvPath, "application/pdf");
        pdfContent.Name = CvFileName;


        var message = new ChatMessage(ChatRole.User,
        [
            new TextContent("Read the attached CV and extract its information into the required structure."),
            pdfContent
        ]);

        System.Console.WriteLine($"Sending {CvFileName} to the LLM...\n");

        var response = await agent.RunAsync<CvInfo>(
            message,
            null,
            JsonSerializerOptions.Web,
            null);

        System.Console.WriteLine("Extracted CV Information:");
        StructuredOutputSetup.PrintJson(response.Result);
        System.Console.WriteLine("\n✓ Demo 6 completed successfully!");
    }
}