using OnlyHumans.Acp;

namespace OnlyHumans.Tests.Acp;

public class UnitTest1
{
    [Fact]
    public async void Test1()
    {

        var c = new Client("TestClient", "1.0", 1, ClientCapabilities.Default);
        var ac = new AgentConnection(c, "cmd.exe", "C:\\DevTools\\gemini", "/c gemini-cli.bat --experimental-acp");
        var r = await ac.InitializeAsync(new InitializeRequest
        {
            ClientCapabilities = ClientCapabilities.Default,
            ClientInfo = new Implementation
            {
                Name = "TestClient",
                Version = "1.0"
            },
            ProtocolVersion = 1
        });
        Assert.True(r.IsSuccess);
    }
}
