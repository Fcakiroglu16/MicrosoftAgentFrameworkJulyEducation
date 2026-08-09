using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.API.Workflow;

public static class SequentialOrderWorkflow
{
    public static async Task<List<ChatMessage>> ExecuteAsync(string orderRequest)
    {
        var orderAgent = OrchestrationAgents.GetOrderIntakeAgent();
        var stockCheckAgent = OrchestrationAgents.GetStockCheckAgent();
        var invoiceAgent = OrchestrationAgents.GetInvoiceAgent();

        var workflow = AgentWorkflowBuilder.BuildSequential(new[] { orderAgent, stockCheckAgent, invoiceAgent });

        var messages = new List<ChatMessage> { new(ChatRole.User, orderRequest) };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        List<ChatMessage> result = new();
        var seenEventTypes = new List<string>();
        var executorTextBuffers = new Dictionary<string, System.Text.StringBuilder>();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            seenEventTypes.Add(evt.GetType().Name);

            if (evt is AgentResponseUpdateEvent updateEvt)
            {
                var buffer = executorTextBuffers.TryGetValue(updateEvt.ExecutorId, out var existing)
                    ? existing
                    : executorTextBuffers[updateEvt.ExecutorId] = new System.Text.StringBuilder();
                buffer.Append(updateEvt.Update.Text);
            }

            if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>() ?? new List<ChatMessage>();
                break;
            }
        }

        var executorSummary = string.Join(
            " | ",
            executorTextBuffers.Select(kvp => $"{kvp.Key}: {kvp.Value.Length} karakter"));

        if (result.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workflow bir sonuç üretmeden tamamlandı. Alınan event'ler: {string.Join(", ", seenEventTypes)}. " +
                $"Executor çıktıları: {(executorSummary.Length == 0 ? "hiçbiri içerik üretmedi" : executorSummary)}");
        }

        return result;
    }
}
