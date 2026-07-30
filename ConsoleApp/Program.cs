using App.Console.Agents;
using App.Console.McpToolCalling;
using Microsoft.Agents.AI;

Console.WriteLine("Microsoft Extensions AI Chat Client");


//await MicrosoftLearnMcp.RunAsync(Environment.GetEnvironmentVariable("OPEN_AI_KEY")!);
await LocalStdioMcpAgent.RunAsync(Environment.GetEnvironmentVariable("OPEN_AI_KEY")!);