using App.Console.Agents;
using App.Console.Workflow;
using App.Console.Workflow.AgentWorkflow;
using App.Console.Workflow.AgentsInWorkflow;
using App.Console.Workflow.EdgeTypes;
using App.Console.Workflow.Mutability;
using App.Console.Workflow.State;
using App.Console.Workflow.WorkflowAsAgent;


Console.WriteLine("Workflow");

//await TextWorkflowSample.Run();

//await DirectEdgeSample.Run();

//await ConditionalEdgeSample.Run();

//await SwitchCaseSample.Run();

//await FanOutSample.Run();

//await FanInSample.Run();

//await MutableWorkflowBuilderSample.Run();

//await ImmutableWorkflowSample.Run();

//await StateSharingSample.Run();

//wait AgentWorkflowSample.Run();

//await AgentsInWorkflowSample.Run();

await WorkflowAsAgentSample.Run();
