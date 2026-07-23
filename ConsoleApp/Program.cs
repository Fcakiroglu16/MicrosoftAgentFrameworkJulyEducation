using App.Console;
using App.Console.Agents;
using App.Console.Agents.MessageTypes;
using App.Console.Agents.StructuredOutput;
using App.Console.Agents.Tools;
using Microsoft.Agents.AI;

Console.WriteLine("Microsoft Extensions AI Chat Client");

//await FunctionTool.Run();

//var agent= AgentSetup.GetAgent();
//await TextContentInResponseAgent.RunAsync(agent);
//await AllResponseContentAgent.RunAsync(agent);
//await MultiModelContentAgent.RunAsync(agent);
//await CvExtraction.RunAsync();
// #pragma warning disable OPENAI001
// await CodeInterpreterHostedTool.RunAsync();
// #pragma warning restore OPENAI001
await ToolApproval.RunAsync();