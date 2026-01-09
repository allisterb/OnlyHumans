namespace OnlyHumans.Understanding.Tests;

public class TestsRuntime : OnlyHumans.TestsRuntime
{
    static TestsRuntime()
    {
        Initialize("OnlyHumans", "Tests", true);
        functiongemmaQ8ModelPath = config.GetRequiredValue("FunctionGemmaQ8ModelPath");
        functiongemmaQ8Model = new Model(ModelRuntime.LlamaCpp, "function-gemma-q8", functiongemmaQ8ModelPath);
    }
    static protected readonly string functiongemmaQ8ModelPath;
    static protected readonly Model functiongemmaQ8Model; 
}

