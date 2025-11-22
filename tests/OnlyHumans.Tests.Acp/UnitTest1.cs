using OnlyHumans.Acp;
using System.Diagnostics;

namespace OnlyHumans.Tests.Acp;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {

        var c = new Client("TestClient", "1.0", 1, ClientCapabilities.Default);
        //var ac = new AgentConnection(c, "cmd.exe", "/c node node_modules\\@google\\gemini-cli\\dist\\index.js --experimental-acp --yolo", "C:\\DevTools\\gemini2", new ConsoleTraceListener());
        using var ac = new AgentConnection(c, "C:\\Users\\Allister\\.local\\bin\\kimi.exe", "--acp --yolo", "C:\\DevTools\\kimi", new ConsoleTraceListener());
        CancellationTokenSource cts = new CancellationTokenSource(5000);    
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
