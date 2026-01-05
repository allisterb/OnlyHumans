namespace OnlyHumans;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

public class AgentManager : Runtime
{
    static AgentManager()
    {
        config = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("testappsettings.json", optional: false, reloadOnChange: false)
           .Build();
        kbdir = config?["Files:KBDir"] ?? Path.Combine(CurentDirectory, "data", "kb");
    }
    public AgentManager(Model textModel, Model embeddingModel)
    {       
        this.textModel = textModel;
        this.embeddingModel = embeddingModel;               
        this.memory = new Memory(textModel, embeddingModel);
        sharedState["Config"] = new();       
    }

    public async Task CreateKBAsync()
    {
        var op = Begin("Indexing documents in KB dir {0}.", kbdir);
        await memory.CreateKBAsync(kbdir);
        op.Complete();
    }

    public AgentConversation StartUserSession(string prompt, params (IPlugin, string)[] plugins)
    {
        var c = new AgentConversation(textModel, embeddingModel, prompt, "Startup Agent", plugins:plugins, systemPrompts: systemPrompts)
        {
            SharedState = sharedState 
        };
        return c;
    }

    #region Fields
    public Dictionary<string, Dictionary<string, object>> sharedState = new()
    {
        { "Agent", new() }
    };

    public List<AgentConversation> conversations = new List<AgentConversation>();

    Memory memory;

    static IConfigurationRoot? config;

    static readonly string[] systemPrompts = [
        "You are working for OnlyHumans, a document intelligence agent that assists blind users with getting information from printed and electronic documents and using this information to interface with different business systems and processes. " +
        "Your users are employees who are vision-impaired so keep your answers as short and precise as possible." + 
        "Your main role is to work on business documents at a console. Only one file at a time will be active in the console." +
        "ONLY use function calls to respond to the user's query on files and documents. If you do not know the answer then inform the user.",
        ];

    Model textModel;

    Model embeddingModel;

    static string kbdir;
    #endregion
}

