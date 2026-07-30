using App.Console.Agents;
using App.Console.McpToolCalling;
using App.Console.McpToolCalling.Agents;
using App.Console.McpToolCalling.AgentsWithStreamableHttp;
using Microsoft.Agents.AI;

Console.WriteLine("Microsoft Extensions AI Chat Client");



//await McpToolAgent.RunAsync(Environment.GetEnvironmentVariable("OPEN_AI_KEY")!);
//await McpResourceAgent.RunAsync(Environment.GetEnvironmentVariable("OPEN_AI_KEY")!);
//await McpPromptAgent.RunAsync(Environment.GetEnvironmentVariable("OPEN_AI_KEY")!);


await McpToolAgentWithStreamableHttp.RunAsync(Environment.GetEnvironmentVariable("OPEN_AI_KEY")!);