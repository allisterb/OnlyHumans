namespace OnlyHumans.Acp;

using System.Net.Mime;

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

public partial record Implementation
{
    public static Implementation Default = new Implementation()
    {
        Name = null,
        Version = "1.0",
        Title = null
    };
}   

public partial record PromptRequest : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.User;

    [JsonIgnore]
    public string Message => Prompt.Select(e => e.Message).JoinWith(Environment.NewLine);
}

public partial record PromptResponse : ITurn
{
    [JsonIgnore]
    public Role Role { get; } = Role.Assistant;

    [JsonIgnore]
    public string Message => StopReason;
}

public partial record ContentBlock 
{
    [JsonIgnore]
    public string Message => this switch { 
        ContentBlockText text => text.Text,
        ContentBlockImage image => image.Data,
        ContentBlockAudio audio => audio.Data,  
        ContentBlockResource resource when resource.Resource is TextResourceContents t => t.Text,
        ContentBlockResource resource when resource.Resource is BlobResourceContents t => t.Blob,
        ContentBlockResourceLink link => link.Uri,
        _ => throw new NotImplementedException()
    };
    
    public static ContentBlockText _Text(string text) => new () { Text = text };   

    public static ContentBlockResource TextResource(string mimeType, string text, Uri uri) 
        => new ContentBlockResource() { Resource = new TextResourceContents() { MimeType = mimeType, Text = text, Uri = uri.ToString() } };
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


public static class RoleExtensions
{
    public static Role Switch(this Role role) => role switch
    {
        Role.User => Role.Assistant,
        _ => Role.User,          
    };
}   