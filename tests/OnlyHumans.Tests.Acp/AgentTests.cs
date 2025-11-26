namespace OnlyHumans.Tests.Acp;

using Microsoft.Extensions.Configuration;
using OnlyHumans.Acp;
using System.Diagnostics;

public class AgentTests : TestsRuntime
{   
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

    [Fact]
    public async Task CanCreateSession()
    {
        using var agent = new Agent(agentCmdPath, agentCmdArgs, agentCmdWd, "TestClient");
        agent.connection.TraceLevel = SourceLevels.Verbose;
        agent.connection.TraceListeners.Add(new ConsoleTraceListener());
        var s = await agent.NewSessionAsync("C:\\DevTools\\kimi");
        Assert.True(s.IsSuccess);
    }
}
