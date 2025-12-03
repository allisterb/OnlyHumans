namespace OnlyHumans.Acp.Tests;

using System.Diagnostics;

public class AgentTests : TestsRuntime
{   
    [Fact]
    public async Task CanCreateAgent()
    {
        using var agent = new Agent(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        var r = await agent.InitializeAsync();
        Assert.True(r.IsSuccess);
        Assert.True(agent.IsInitialized);
    }

    [Fact]
    public async Task CanCreateSession()
    {
        using var agent = new Agent(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        var s = await agent.NewSessionAsync(agentCmdWd);
        Assert.True(s.IsSuccess);
        Assert.NotNull(s.Value.sessionId);
    }

    [Fact]
    public async Task CanAuthenticate()
    {
        using var agent = new Agent(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        var r = await agent.InitializeAsync();
        var ar = await agent.AuthenticateAsync("api_key", new Dictionary<string, object> { { "apiKey", agentApiKey } });
        Assert.True(ar.IsSuccess);  
    }

    [Fact]
    public async Task CanSetModel()
    {
        using var agent = new Agent(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        await agent.InitializeAsync();
        var ar = await agent.NewSessionAsync(agentCmdWd);
        Assert.True(ar.IsSuccess);
        var mr = await agent.SetSessionModelAsync(new SetSessionModelRequest() { SessionId = ar.Value.sessionId, ModelId = agentModel });
        Assert.True(mr.IsSuccess);
        var pr = await agent.PromptAsync(ar.Value.sessionId, "hello kimi");
        Assert.True(pr.IsSuccess);
    }
}
