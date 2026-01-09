namespace OnlyHumans.Understanding.Tests;

public class ModelTests : TestsRuntime
{
    [Fact]
    public async Task CanLoadLlamaSharpModel()
    {
        var p1 = new TestPlugin();
        var mc = new ModelConversation(functiongemmaQ8Model, systemPrompts: ["You are a model that can do function calling with the following functions"], plugins: [(p1, "math")]);
        Assert.NotNull(mc);

        //mc.AddPlugin(p1, "testplugin");
        var m = mc.Prompt("Add the integers 3 and 5?");
        await foreach (var message in m)
        {
            Console.WriteLine(message);
        }
        Assert.NotEmpty(mc.messages);

    }
}
