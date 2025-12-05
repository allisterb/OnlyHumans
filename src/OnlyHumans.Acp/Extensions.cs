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

public partial record PromptRequest : IContentBlock
{
    [JsonIgnore]
    public string Contents => Prompt.Select(e => e.Contents).JoinWith(Environment.NewLine);
}

public partial record ContentBlock : IContentBlock
{
    public static ContentBlockText _Text(string text) => new ContentBlockText() { Text = text };

    [JsonIgnore]
    public string Contents => this switch { 
        ContentBlockText text => text.Text,
        ContentBlockImage image => image.Data,
        ContentBlockAudio audio => audio.Data,  
        ContentBlockResource resource => resource.ToString(),
        ContentBlockResourceLink link => link.Uri,
        _ => throw new NotImplementedException()
    };
}

public partial record SessionUpdatePlan : IContentBlock
{
    [JsonIgnore]
    public string Contents => Entries.Select(e => e.Content).JoinWith(Environment.NewLine);  
}

public partial record SessionUpdateToolCall : IContentBlock
{
    [JsonIgnore]
    public string Contents => $"{Kind} {Title}({ToolCallId})";
}

public partial record SessionUpdateToolCallUpdate : IContentBlock
{
    [JsonIgnore] 
    public string Contents => $"{Kind} {Title}({ToolCallId}) {Status}";
}

public partial record StopReason
{        
    public const string EndTurn = "end_turn";   
}