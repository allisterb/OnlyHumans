namespace OnlyHumans;

using Microsoft.Extensions.Configuration;   

public class TestsRuntime : Runtime
{
    static TestsRuntime()
    {        
        Initialize("OnlyHumans", "Tests", true);
        config = LoadConfigFile("testappsettings.json");
        agentCmdPath = GetRequiredConfigValue(config, "AgentCmdPath");
        agentCmdArgs = GetRequiredConfigValue(config, "AgentCmdArgs");
        agentCmdWd = GetRequiredConfigValue(config, "AgentCmdWd");
    }

    static protected IConfigurationRoot config;
    static protected string agentCmdPath, agentCmdArgs, agentCmdWd;
}

