namespace OnlyHumans.Acp;



public class Agent : Runtime
{
    #region Constructors
    public Agent(AgentConnection agentConnection, Implementation clientInfo, ClientCapabilities clientCapabilities)
    {
        this.agentConnection = agentConnection;
        this.clientInfo = clientInfo;
        this.clientCapabilities = clientCapabilities;
    }
    
    public Agent(AgentConnection agentConnection, string clientName, string clientVersion = "1.0", string? clientTitle=null)
        : this(agentConnection, new Implementation()
        {
            Name = clientName,
            Version = clientVersion,
            Title = clientTitle
        }, ClientCapabilities.Default) {}
    #endregion

    #region Methods
    public async Task<Result<bool>> InitializeAsync(CancellationToken cancellationToken) =>
        await agentConnection.InitializeAsync(new InitializeRequest { ClientCapabilities = clientCapabilities, ClientInfo = clientInfo, ProtocolVersion = 1 }, cancellationToken)
        .Map(Initialize);

    public async Task<Result<Session>> NewSessionAsync(string cwd) => 
        await agentConnection.NewSessionAsync(new NewSessionRequest() { Cwd = cwd })
        .Map(NewSession);

    protected bool Initialize(InitializeResponse r)
    {
        this.agentInfo = r.AgentInfo;
        this.agentCapabilities = r.AgentCapabilities;
        return true;
    }

    protected Session NewSession(NewSessionResponse r)
    {
        var s = new Session(this, r.SessionId, r);   
        sessions.Add(r.SessionId, s);
        return s;
    }
    #endregion
    
    #region Fields

    protected readonly AgentConnection agentConnection;
    protected readonly ClientCapabilities clientCapabilities;
    protected readonly Implementation clientInfo;
    public readonly Dictionary<string, Session> sessions = new Dictionary<string, Session>();
    public AgentCapabilities? agentCapabilities = null;
    public AgentInfo? agentInfo = null;
    protected ulong sessionCounter = 0;
    protected ulong terminalCounter = 0;
    #endregion

   
}
