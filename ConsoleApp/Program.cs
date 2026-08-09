
using App.Console.Orchestrations.Handoff;


Console.WriteLine("Orchestration Workflow");

//await SequentialOrderWorkflow.RunAsync();
//await ConcurrentTranslationWorkflow.RunAsync();
await HandoffSupportWorkflow.RunAsync();



