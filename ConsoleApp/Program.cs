
using App.Console.Orchestrations.Concurrent;


Console.WriteLine("Orchestration Workflow");

//await SequentialOrderWorkflow.RunAsync();
await ConcurrentTranslationWorkflow.RunAsync();



