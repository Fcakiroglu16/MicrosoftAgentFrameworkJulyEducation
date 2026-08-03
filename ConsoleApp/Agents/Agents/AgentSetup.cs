using App.Console.Workflow.AgentWorkflow;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents;

public static class AgentSetup
{
    private static IChatClient CreateChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Please set the OPEN_AI_KEY environment variable.");

        return new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient();
    }

    public static ChatClientAgent GetAgent()
    {
        var chatClient = CreateChatClient();


        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "GeneralAssistant",
            Description = "A general-purpose AI assistant that answers users' questions.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are a general-purpose AI assistant. Your sole purpose is to answer user questions.
                               - Answer any question on any topic clearly and accurately.
                               - Keep answers concise but complete; elaborate only when the user asks.
                               - If you do not know something, say so honestly instead of guessing.
                               - Do not refuse reasonable questions; always try to be helpful.
                               """
            }
        });
    }

    public static ChatClientAgent GetTranslationAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "TurkishToEnglishTranslator",
            Description = "Translates Turkish sentences into English.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are a professional Turkish-to-English translator.
                               - Translate the user's Turkish sentence into natural, fluent English.
                               - Return ONLY the translated sentence, without quotes, labels, or extra commentary.
                               """
            }
        });
    }

    public static ChatClientAgent GetSentimentAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "SentimentClassifier",
            Description = "Classifies a sentence as Positive or Negative with structured output.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are a sentiment analysis assistant.
                               - Read the given sentence and decide whether its overall sentiment is Positive or Negative.
                               - Provide a short reasoning for your decision.
                               - Respond using only the requested structured format.
                               """
            }
        });
    }

    /// <summary>
    /// Creates a translation agent that translates whatever text it receives into the
    /// specified target language, mirroring the "Agents in Workflows" tutorial pattern.
    /// </summary>
    public static ChatClientAgent GetTranslationAgent(string targetLanguage)
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = $"{targetLanguage}Translator",
            Description = $"Translates the provided text into {targetLanguage}.",
            ChatOptions = new ChatOptions
            {
                Instructions = $"You are a translation assistant that translates the provided text to {targetLanguage}."
            }
        });
    }

    /// <summary>
    /// Creates an agent whose sole job is to convert the received text to upper case.
    /// Used to demonstrate a fully agent-based workflow, where every node - including
    /// this simple transformation step - is an AIAgent instead of a plain executor.
    /// </summary>
    public static ChatClientAgent GetUpperCaseAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "UpperCaser",
            Description = "Converts the provided text to upper case.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               Convert the given text to ALL UPPERCASE.
                               Return ONLY the uppercased text, without quotes, labels, or extra commentary.
                               """
            }
        });
    }

    /// <summary>
    /// Creates a sentiment classification agent that returns its verdict as structured
    /// JSON output (matching <see cref="SentimentResult"/>) via the agent's own
    /// <see cref="ChatOptions.ResponseFormat"/>. Used when the agent is placed directly
    /// in a workflow, where structured output can't be requested through RunAsync&lt;T&gt;.
    /// </summary>
    public static ChatClientAgent GetStructuredSentimentAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "SentimentClassifier",
            Description = "Classifies a sentence as Positive or Negative with structured output.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are a sentiment analysis assistant.
                               - Read the given sentence and decide whether its overall sentiment is Positive or Negative.
                               - Provide a short reasoning for your decision.
                               """,
                ResponseFormat = ChatResponseFormat.ForJsonSchema<SentimentResult>()
            }
        });
    }

    public static string? Truncate(string? text, int maxLength = 80)
    {
        return text?.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength), "...") : text;
    }
}