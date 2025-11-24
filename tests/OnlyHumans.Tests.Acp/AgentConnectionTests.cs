namespace OnlyHumans.Tests.Acp;

using System.Diagnostics;

using OnlyHumans.Acp;

public class AgentConnectionTests
{
    [Fact]
    public async Task CanInitializeAgent()
    {
        //var ac = new AgentConnection("cmd.exe", "/c node node_modules\\@google\\gemini-cli\\dist\\index.js --experimental-acp --yolo", "C:\\DevTools\\gemini2", SourceLevels.Verbose, new ConsoleTraceListener());
        using var ac = new AgentConnection("C:\\Users\\Allister\\.local\\bin\\kimi.exe", "--acp --yolo", "C:\\DevTools\\kimi", SourceLevels.Verbose, new ConsoleTraceListener());
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
    }
}
