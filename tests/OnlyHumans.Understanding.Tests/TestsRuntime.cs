namespace OnlyHumans.Understanding.Tests;

using Microsoft.Extensions.Configuration;

public class TestsRuntime : OnlyHumans.TestsRuntime
{
    static TestsRuntime()
    {
        Initialize("OnlyHumans", "Tests", true);
        functionGemmaQ8ModelPath = config.GetRequiredValue("FunctionGemmaQ8ModelPath");
        functionGemmaQ8Model1 = new Model(ModelRuntime.LlamaCpp, "function-gemma-q8", functionGemmaQ8ModelPath);
    }
    static protected readonly string functionGemmaQ8ModelPath;
    static protected readonly Model functionGemmaQ8Model1; 
}

