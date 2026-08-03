using App.Console.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.WorkflowAsAgent;

/// <summary>
/// Demonstrates converting a multi-agent sequential workflow into a single <see cref="AIAgent"/>
/// via <see cref="WorkflowHostingExtensions.AsAIAgent"/>, so it can be invoked/consumed just like
/// any other agent (e.g. RunAsync, or nested inside another workflow).
/// </summary>
public static class WorkflowAsAgentSample
{
    private const string Input = "The benefits of remote work for software teams";

    public static async Task Run()
    {
        AIAgent researchAgent = AgentSetup.GetResearchAgent();
        AIAgent writerAgent = AgentSetup.GetWriterAgent();
        AIAgent reviewerAgent = AgentSetup.GetReviewerAgent();
        var wordCountExecutor = new WordCountLoggingExecutor();

        // Build a sequential workflow
        var workflow = new WorkflowBuilder(researchAgent)
            .AddEdge(researchAgent, writerAgent)
            .AddEdge(writerAgent, wordCountExecutor)
            .AddEdge(wordCountExecutor, reviewerAgent)
            .WithOutputFrom(reviewerAgent)
            .Build();

        // Convert the workflow to an agent
        AIAgent workflowAgent = workflow.AsAIAgent(
            id: "content-pipeline",
            name: "Content Pipeline Agent",
            description: "A multi-agent workflow that researches, writes, and reviews content");

        // The workflow agent can now be used just like any other AIAgent.
        var response = await workflowAgent.RunAsync(Input);

        System.Console.WriteLine("--- Content Pipeline Agent Output ---");
        System.Console.WriteLine(response.Text);
    }
}
