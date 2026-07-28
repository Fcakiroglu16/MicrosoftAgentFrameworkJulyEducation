using App.Console.Agents;
using Microsoft.Agents.AI;

Console.WriteLine("Microsoft Extensions AI Chat Client");


var agent= AgentSetup.GetAgent();
// Create and reuse a session

AgentSession session = await agent.CreateSessionAsync();

var first = await agent.RunAsync("benim ismim fatih", session);
Console.WriteLine(first.Text);

var second = await agent.RunAsync("ismim neydi?", session);
Console.WriteLine(second.Text);

// Persist and restore later
var serialized = await agent.SerializeSessionAsync(session);
Console.WriteLine(serialized.ToString());

AgentSession resumed = await agent.DeserializeSessionAsync(serialized);
