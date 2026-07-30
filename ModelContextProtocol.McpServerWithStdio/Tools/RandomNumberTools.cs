using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.McpServerWithStdio.Tools;

internal class RandomNumberTools
{
    [McpServerTool(Name = "get_random_number")]
    
    [Description("Generates a random number between the specified minimum and maximum values.")]
    public int GetRandomNumber(
        [Description("Minimum value (inclusive)")] int min = 0,
        [Description("Maximum value (exclusive)")] int max = 100)
    {
        return Random.Shared.Next(min, max);
    }

}