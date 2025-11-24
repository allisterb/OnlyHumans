namespace OnlyHumans.Acp;

public partial record ClientCapabilities
{
    public static ClientCapabilities Default { get; } = new ClientCapabilities()
    {
        Fs = new FileSystemCapability()
        {
            ReadTextFile = true,
            WriteTextFile = true
        },
        Terminal = true,
    };
}

public class Agent : Runtime
{
    public Agent(AgentConnection agentConnection)
    {
        this.agentConnection = agentConnection;
    }

    #region Methods
    public async Task<Result<Session>> NewSessionAsync(string cwd) => (await agentConnection.NewSessionAsync(new NewSessionRequest()
    {
        Cwd = cwd
    }))
    .Map(NewSession);

    #endregion

    #region Methods
    protected Session NewSession(NewSessionResponse r)
    {
        var s = new Session(this, r.SessionId, r);   
        sessions.Add(r.SessionId, s);
        return s;
    }
    #endregion
    
    #region Fields

    protected readonly AgentConnection agentConnection;
    public readonly Dictionary<string, Session> sessions = new Dictionary<string, Session>();
    protected ulong sessionCounter = 0;
    protected ulong terminalCounter = 0;
    #endregion

   
}
