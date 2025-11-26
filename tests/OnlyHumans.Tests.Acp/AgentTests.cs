namespace OnlyHumans.Tests.Acp;

using Microsoft.Extensions.Configuration;
using OnlyHumans.Acp;
using System.Diagnostics;

public class AgentTests : Runtime
{
    static AgentTests()
    {
        config = LoadConfigFile("appsettings.json"); 
        agentCmdPath = GetRequiredConfigValue(config, "AgentCmdPath");
        agentCmdArgs = GetRequiredConfigValue(config, "AgentCmdArgs");
        agentCmdWd = GetRequiredConfigValue(config, "AgentCmdWd");
    }

    [Fact]
    public async Task CanCreateAgent()
    {
        using var agent = new Agent(agentCmdPath, agentCmdArgs, agentCmdWd, "TestClient");
        agent.connection.TraceLevel = SourceLevels.Verbose;
        agent.connection.TraceListeners.Add(new ConsoleTraceListener());
        var r = await agent.InitializeAsync();
        Assert.True(r.IsSuccess);
        Assert.True(agent.IsInitialized);
    }

    internal static IConfigurationRoot config;
    internal static string agentCmdPath, agentCmdArgs, agentCmdWd;
}
