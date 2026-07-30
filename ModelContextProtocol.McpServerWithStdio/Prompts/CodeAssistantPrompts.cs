using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.McpServerWithStdio.Prompts;

[McpServerPromptType]
internal class CodeAssistantPrompts
{
    // ── Code review ───────────────────────────────────────────────────────────

    [McpServerPrompt]
    [Description("Generates a thorough code-review prompt for a given language and code snippet.")]
    public static IEnumerable<ChatMessage> CodeReview(
        [Description("The programming language (e.g. C#, Python, TypeScript)")] string language,
        [Description("The code snippet to review")] string code) =>
    [
        new(ChatRole.User,
            $"""
             Please review the following {language} code and comment on:
             1. Correctness — logic errors, edge cases, null handling
             2. Style — naming, formatting, idiomatic patterns for {language}
             3. Performance — obvious inefficiencies or unnecessary allocations
             4. Security — potential vulnerabilities (e.g. injection, overflow)

             ```{language}
             {code}
             ```
             """),
        new(ChatRole.Assistant,
            $"I'll review the {language} code now and address each of the four areas you listed.")
    ];

    // ── Bug explanation ───────────────────────────────────────────────────────

    [McpServerPrompt]
    [Description("Asks the assistant to explain and fix a bug in a code snippet.")]
    public static IEnumerable<ChatMessage> ExplainBug(
        [Description("The programming language")] string language,
        [Description("The buggy code")] string code,
        [Description("The error message or unexpected behaviour observed")] string errorMessage) =>
    [
        new(ChatRole.User,
            $"""
             I have the following {language} code that produces an error:

             ```{language}
             {code}
             ```

             Error / unexpected behaviour:
             > {errorMessage}

             Please:
             1. Explain the root cause of the bug in plain language.
             2. Show a corrected version of the code.
             3. Suggest how to prevent this class of bug in the future.
             """),
        new(ChatRole.Assistant,
            "Let me analyse the error, identify the root cause, and provide a corrected version.")
    ];


   
}