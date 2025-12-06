namespace OnlyHumans.Acp;

using Newtonsoft.Json;

public partial record ClientCapabilities
{
    public static ClientCapabilities Default = new ClientCapabilities()
    {
        Fs = new FileSystemCapability()
        {
            ReadTextFile = true,
            WriteTextFile = true
        },
        Terminal = true,
    };
}

public partial record PromptRequest : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.User;

    [JsonIgnore]
    public string Message => Prompt.Select(e => e.Message).JoinWith(Environment.NewLine);
}

public partial record ContentBlock 
{
    [JsonIgnore]
    public string Message => this switch { 
        ContentBlockText text => text.Text,
        ContentBlockImage image => image.Data,
        ContentBlockAudio audio => audio.Data,  
        ContentBlockResource resource => resource.ToString(),
        ContentBlockResourceLink link => link.Uri,
        _ => throw new NotImplementedException()
    };

    public static ContentBlockText _Text(string text) => new () { Text = text };   
}

public partial record SessionUpdateAgentMessageChunk : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.Assistant;
    [JsonIgnore]
    public string Message => Content.Message;
}

public partial record SessionUpdatePlan : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.Assistant;
    
    [JsonIgnore]
    public string Message => Entries.Select(e => e.Content).JoinWith(Environment.NewLine);  
}

public partial record SessionUpdateToolCall : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.Assistant;
    [JsonIgnore]
    public string Message => $"{Kind} {Title}({ToolCallId})";
}

public partial record SessionUpdateToolCallUpdate : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.Assistant;

    [JsonIgnore] 
    public string Message => $"{Kind} {Title}({ToolCallId}) {Status}";
}

