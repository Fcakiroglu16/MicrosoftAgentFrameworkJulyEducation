using App.Console.Agents;
using App.Console.Workflow;
using App.Console.Workflow.EdgeTypes;
using App.Console.Workflow.Mutability;


Console.WriteLine("Workflow");

//await TextWorkflowSample.Run();

//await DirectEdgeSample.Run();

//await ConditionalEdgeSample.Run();

//await SwitchCaseSample.Run();

//await FanOutSample.Run();

//await FanInSample.Run();

await MutableWorkflowBuilderSample.Run();

await ImmutableWorkflowSample.Run();


