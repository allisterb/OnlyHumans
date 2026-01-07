namespace OnlyHumans;

using Microsoft.Extensions.Configuration;   

public class TestsRuntime : Runtime
{
    static TestsRuntime()
    {
        Initialize("OnlyHumans", "Tests", true);
        config = LoadConfigFile("testappsettings.json");
    }    
    static protected IConfigurationRoot config;
}

