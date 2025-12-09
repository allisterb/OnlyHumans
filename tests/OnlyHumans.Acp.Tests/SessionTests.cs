namespace OnlyHumans.Acp.Tests;

using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;

public class SessionTests : TestsRuntime
{
    static SessionTests()
    {
        client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd)
            .WithVerboseConsoleConnectionTracing();
        client.InitializeAsync().Succeeded().Wait();
    }
    
    [Fact]
    public async Task CanPrompt()
    {
        var sess = await client.NewSessionAsync(agentCmdWd).Succeeded();
        Assert.True(sess.CurrentTurn == Role.User);
        var ar = await sess.PromptAsync("Hello").Succeeded();
        var br = await sess.PromptAsync([
            ContentBlock._Text("Can you analyze this code for potential issues?"),
            ContentBlock.TextResource("text/x-python", "def process_data(items):\n    for item in items:\n        print(item)", new Uri("file:///home/user/project/main.py")) 
        ]);
        Assert.True(br.IsSuccess);

    }
    
    static Client client;
}
