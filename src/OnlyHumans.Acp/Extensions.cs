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

public partial record ContentBlock
{
    public static ContentBlockText _Text(string text) => new ContentBlockText() { Text = text };    
}
