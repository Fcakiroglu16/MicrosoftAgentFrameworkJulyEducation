using App.Console.Agents;
using App.Console.Orchestrations.Sequential;


Console.WriteLine("Workflow");

await SequentialOrderWorkflow.RunAsync();



