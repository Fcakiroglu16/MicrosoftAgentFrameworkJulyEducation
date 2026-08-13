using App.Console.Orchestrations.Magentic;

Console.WriteLine("Orchestration Workflow");

//await SequentialOrderWorkflow.RunAsync();


//await ConcurrentTranslationWorkflow.RunAsync();
//await HandoffSupportWorkflow.RunAsync();
//await GroupChatSloganWorkflow.RunAsync();
await MagenticBusinessPlanWorkflow.RunAsync();