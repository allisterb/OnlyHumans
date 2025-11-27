namespace OnlyHumans.Tests.Acp;

using System.Diagnostics;

using OnlyHumans.Acp;

public class AgentConnectionTests : TestsRuntime
{   
    [Fact]
    public async Task CanInitializeAgent()
    {
        using var ac = new AgentConnection(agentCmdPath, agentCmdArgs, agentCmdWd, SourceLevels.Verbose, new ConsoleTraceListener());

        CancellationTokenSource cts = new CancellationTokenSource(50000);    
        CancellationToken token = cts.Token;    
        var r = await ac.InitializeAsync(new InitializeRequest
        {
            ClientCapabilities = ClientCapabilities.Default,
            ClientInfo = new Implementation
            {
                Name = "TestClient",
                Version = "1.0"
            },
            ProtocolVersion = 1
        }, token);
        
        Assert.True(r.IsSuccess);
        ac.Dispose();
    }

    
}
