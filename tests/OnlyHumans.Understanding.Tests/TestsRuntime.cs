namespace OnlyHumans.Understanding.Tests;

public class TestsRuntime : OnlyHumans.TestsRuntime
{
    static TestsRuntime()
    {
        Initialize("OnlyHumans", "Tests", true);
        ModelConversation.config = config;
        functiongemmaQ8ModelPath = config.GetRequiredValue("FunctionGemmaQ8ModelPath");
        
        functiongemmaQ8Model = new Model(ModelRuntime.LlamaCpp, "function-gemma-q8", functiongemmaQ8ModelPath);
        gemma3ProModel = new Model(ModelRuntime.GoogleGemini, "gemini-3-pro-preview");
    }
    static protected readonly string functiongemmaQ8ModelPath;
    static protected readonly Model functiongemmaQ8Model;
    static protected readonly Model gemma3ProModel;

}

