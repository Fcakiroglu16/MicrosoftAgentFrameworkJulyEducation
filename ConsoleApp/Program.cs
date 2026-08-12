
using App.Console.Orchestrations.Concurrent;
using App.Console.Orchestrations.GroupChat;
using App.Console.Orchestrations.Handoff;
using App.Console.Orchestrations.Magentic;
using App.Console.Orchestrations.Sequential;


Console.WriteLine("Orchestration Workflow");

//await SequentialOrderWorkflow.RunAsync();


//await ConcurrentTranslationWorkflow.RunAsync();
//await HandoffSupportWorkflow.RunAsync();
//await GroupChatSloganWorkflow.RunAsync();
await MagenticBusinessPlanWorkflow.RunAsync();



