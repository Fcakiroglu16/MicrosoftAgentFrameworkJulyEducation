namespace App.Console.Workflow.AgentWorkflow;

/// <summary>
/// Structured output produced by the sentiment classification agent.
/// </summary>
public sealed record SentimentResult(string Sentiment, string Reasoning);
