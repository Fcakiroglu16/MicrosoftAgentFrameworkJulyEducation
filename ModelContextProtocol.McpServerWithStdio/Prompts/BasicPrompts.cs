using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.McpServerWithStdio.Prompts;

[McpServerPromptType]
internal class BasicPrompts
{

    [McpServerPrompt]
    [Description("A friendly greeting that opens a general-purpose help conversation.")]
    public static ChatMessage Greeting()
        => new(ChatRole.User, "Hello! How can you help me today?");
    
    [McpServerPrompt]
    [Description("Opens a conversation in which the assistant acts as a concise summariser.")]
    public static IEnumerable<ChatMessage> SummariserMode() =>
    [
        new(ChatRole.System,    "You are a concise summariser. Always reply in 3 bullet points or fewer."),
        new(ChatRole.User,      "I'm ready. Please summarise the next piece of text I send you."),
        new(ChatRole.Assistant, "Sure — go ahead and paste the text you'd like me to summarise.")
    ];
    

    [McpServerPrompt]
    [Description("Opens a brainstorming session with the model acting as a creative partner.")]
    public static IEnumerable<ChatMessage> Brainstorm() =>
    [
        new(ChatRole.System,    "You are a creative partner. Generate diverse, unconventional ideas."),
        new(ChatRole.User,      "Let's brainstorm. I'll describe a problem and you'll suggest ideas."),
        new(ChatRole.Assistant, "Great! Describe the problem or topic and I'll suggest several fresh angles.")
    ];

    
    [McpServerPrompt]
    [Description("Asks the assistant to explain any topic in the simplest possible terms.")]
    public static ChatMessage ExplainSimply()
        => new(ChatRole.User, "Please explain the next concept I give you as if I were five years old.");
}