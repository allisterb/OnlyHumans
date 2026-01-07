namespace OnlyHumans.Understanding.Tests;

using OnlyHumans.Understanding;
public class ModelTests : TestsRuntime
{
    [Fact]
    public async Task CanLoadLlamaSharpModel()
    {
        var mc = new ModelConversation(functionGemmaQ8Model1);
        Assert.NotNull(mc);

    }
}
