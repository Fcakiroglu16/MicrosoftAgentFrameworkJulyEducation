using App.Console.Agents.Tools;

Console.WriteLine("Microsoft Extensions AI Chat Client");


//var agent= AgentSetup.GetAgent();
//await MultiModelContentAgent.RunAsync(agent);
//await AllResponseContentAgent.RunAsync(agent);

//await CvExtraction.RunAsync();
//  #pragma warning disable OPENAI001
// await CodeInterpreterHostedTool.RunAsync();
//  #pragma warning restore OPENAI001
await ToolApproval.RunAsync();
//await FunctionTool.Run();