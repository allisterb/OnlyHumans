namespace OnlyHumans.Acp.Tests;

using System.Diagnostics;

public class ClientTests : TestsRuntime
{   
    [Fact]
    public async Task CanInitialize()
    {
        using var client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        var r = await client.InitializeAsync();
        Assert.True(r.IsSuccess);
        Assert.True(client.IsInitialized);
    }

    [Fact]
    public async Task CanCreateSession()
    {
        using var client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        Assert.True(await client.InitializeAsync().IsSuccess());
        var s = await client.NewSessionAsync(agentCmdWd);
        Assert.True(s.IsSuccess);
        Assert.NotNull(s.Value.sessionId);
    }

    [Fact]
    public async Task CanAuthenticate()
    {
        using var client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
        var r = await client.InitializeAsync();
        var ar = await client.AuthenticateAsync("api_key", new Dictionary<string, object> { { "apiKey", agentApiKey } });
        Assert.True(ar.IsSuccess);  
    }
    
    [Fact]
    public async Task CanPrompt()
    {
        using var client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd, agentEnv, "TestClient")
            .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());       
        Assert.True(await client.InitializeAsync().IsSuccess());
        var session = await client.NewSessionAsync(agentCmdWd).Succeeded();
        var pr = await session.PromptAsync("hello");
        Assert.True(pr.IsSuccess);
        pr = await session.PromptAsync("What is your name?");
        Assert.True(pr.IsSuccess);
    }
}
