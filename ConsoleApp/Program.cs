using App.Console.Agents;
using App.Console.Skills.ClassBased;
using App.Console.Skills.FileBased;
using App.Console.Skills.Inline;

Console.WriteLine("Microsoft Extensions AI Chat Client");


//await CarRentalInlineSkillAgent.Run();
//await CarRentalClassSkillAgent.Run();
await CarRentalFileSkillAgent.Run();
