namespace OnlyHumans.Tests.Acp;

using System.Diagnostics;

using Microsoft.Extensions.Configuration;

using OnlyHumans.Acp;

public class AgentConnectionTests : Runtime
{
    static AgentConnectionTests()
    {
        config = LoadConfigFile("appsettings.json");
        agentCmdPath = GetRequiredConfigValue(config, "AgentCmdPath");
        agentCmdArgs = GetRequiredConfigValue(config, "AgentCmdArgs");
        agentCmdWd = GetRequiredConfigValue(config, "AgentCmdWd");
    }

    [Fact]
    public async Task CanInitializeAgent()
    {        
        using var ac = new AgentConnection(agentCmdPath, agentCmdArgs, agentCmdWd);
        ac.TraceLevel = SourceLevels.Verbose;
        ac.TraceListeners.Add(new ConsoleTraceListener());   
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

    internal static IConfigurationRoot config;
    internal static string agentCmdPath, agentCmdArgs, agentCmdWd;
}
