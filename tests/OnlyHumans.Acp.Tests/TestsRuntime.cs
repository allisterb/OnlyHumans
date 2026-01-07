namespace OnlyHumans.Acp.Tests;

  
public class TestsRuntime : OnlyHumans.TestsRuntime
{
    static TestsRuntime() 
    {            
        agentCmdPath = config.GetRequiredValue("AgentCmdPath");
        agentCmdArgs = config.GetRequiredValue("AgentCmdArgs"); 
        agentCmdWd = config.GetRequiredValue("AgentCmdWd");
        agentApiKey = config.GetRequiredValue("AgentApiKey");
        agentModel = config.GetRequiredValue("AgentModel");
        agentCmdPath2 = config.GetRequiredValue("AgentCmdPath2");
        agentCmdArgs2 = config.GetRequiredValue("AgentCmdArgs2");
        agentCmdWd2 = config.GetRequiredValue("AgentCmdWd2");
    }   
    static protected string agentCmdPath, agentCmdArgs, agentCmdWd, agentApiKey, agentModel, agentCmdPath2, agentCmdArgs2, agentCmdWd2;
    static protected Dictionary<string, string?> agentEnv = new();
}

