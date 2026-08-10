using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.API.Workflow.Sequential;


public class SequentialOrderWorkflow(IChatClient chatClient)
{


    public async Task<List<ChatMessage>> ExecuteAsync(
        string orderRequest)
    {




        var orchestrationAgents = new OrchestrationAgents(chatClient);
        var orderAgent = orchestrationAgents.GetOrderIntakeAgent();
        var stockCheckAgent = orchestrationAgents.GetStockCheckAgent();
        var invoiceAgent = orchestrationAgents.GetInvoiceAgent();

        var workflow = AgentWorkflowBuilder.BuildSequential(new[] { orderAgent, stockCheckAgent, invoiceAgent });

        var messages = new List<ChatMessage> { new(ChatRole.User, orderRequest) };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));


        List<ChatMessage> result = new();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent e)
            {



            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>()!;
                break;
            }
        }

        return result;
    }
}
