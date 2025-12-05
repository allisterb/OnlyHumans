namespace OnlyHumans.Acp;

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
    public string Contents => this.Prompt.Select(e => e.Contents).JoinWith(Environment.NewLine);
}

public partial record ContentBlock : IContentBlock
{
    public static ContentBlockText _Text(string text) => new ContentBlockText() { Text = text };
    public string Contents => this.ToString();
}

public partial record SessionUpdatePlan : IContentBlock
{
    public string Contents => this.Entries.Select(e => e.Content).JoinWith(Environment.NewLine);  
}

public partial record StopReason
{        
    public const string EndTurn = "end_turn";   
}