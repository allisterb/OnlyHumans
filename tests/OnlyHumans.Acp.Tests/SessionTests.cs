namespace OnlyHumans.Acp.Tests;

using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;

public class SessionTests : TestsRuntime
{
    static SessionTests()
    {
        client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd, clientTitle: "TestClient")
                    .WithVerboseConsoleConnectionTracing();
        client.InitializeAsync().Success().Wait();
    }
    
    [Fact]
    public async Task CanPrompt()
    {
        var sess = await client.NewSessionAsync(agentCmdWd).Success();
        Assert.True(sess.CurrentTurn == Role.User);
        var pr = await sess.PromptAsync([
            ContentBlock.TextPrompt("Can you analyze this code for potential issues?"),
            ContentBlock.TextResource("text/x-python", "def process_data(items):\n    for item in items:\n        print(item)", new Uri("file:///home/user/project/main.py")) 
        ]).Success();
        Assert.True(sess.CurrentTurn == Role.User);       
        Assert.NotEmpty(pr.updates);
    }

    [Fact]
    public async Task CanPromptWithSystemPrompt()
    {
        var prompt = File.ReadAllText("SystemPromptA.md");
        var sess = await client.NewSessionAsync(agentCmdWd).Success();
        var pr = await sess.PromptAsync(prompt).Success();
        Assert.NotEmpty(pr.updates);    
        pr = await sess.PromptAsync("Delete the file C:\\Projects\\C-sharp-console-gui-framework\\README.md.").Success();
        Assert.NotEmpty(pr.updates);
    }
    static Client client;
}
