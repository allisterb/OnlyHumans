namespace OnlyHumans.Understanding.Tests;

public class ModelTests : TestsRuntime
{
    [Fact]
    public async Task CanLoadLlamaSharpModel()
    {
        var mc = new ModelConversation(functionGemmaQ8Model);
        Assert.NotNull(mc);
        var m = mc.Prompt("Hello who are you?");
        await foreach(var message in m)
        {
            Console.WriteLine(message);
        }

    }
}
