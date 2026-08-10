using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.API.Workflow;

public static class SequentialOrderWorkflow
{
    public static async Task<List<ChatMessage>> ExecuteAsync(IChatClient chatClient, string orderRequest)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        var orderAgent = OrchestrationAgents.GetOrderIntakeAgent(chatClient);
        var stockCheckAgent = OrchestrationAgents.GetStockCheckAgent(chatClient);
        var invoiceAgent = OrchestrationAgents.GetInvoiceAgent(chatClient);

        var workflow = AgentWorkflowBuilder.BuildSequential(new[] { orderAgent, stockCheckAgent, invoiceAgent });

        var messages = new List<ChatMessage> { new(ChatRole.User, orderRequest) };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        List<ChatMessage> result = new();
        var seenEventTypes = new List<string>();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            seenEventTypes.Add(evt.GetType().Name);

            if (evt is AgentResponseUpdateEvent updateEvt)
            {
                System.Console.Write(updateEvt.Update.Text);
            }

            if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>() ?? new List<ChatMessage>();
                break;
            }
        }


        System.Console.WriteLine();
        foreach (var message in result)
        {
            System.Console.WriteLine($"[{message.Role}] {message.Text}");
        }



        return result;
    }
}
