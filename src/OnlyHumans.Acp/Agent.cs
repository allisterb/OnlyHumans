using System.Diagnostics;

namespace OnlyHumans.Acp;

public class Agent : Runtime, IDisposable
{
    #region Constructors
    public Agent(AgentConnection agentConnection, Implementation clientInfo, ClientCapabilities clientCapabilities, string? name = null)
    {
        this.connection = agentConnection;
        this.clientInfo = clientInfo;
        this.clientCapabilities = clientCapabilities;
        this.Name = name;
    }
    
    public Agent(AgentConnection agentConnection, string clientName, string clientVersion = "1.0", string? clientTitle=null, string? name=null)
        : this(agentConnection, new Implementation() { Name = clientName, Version = clientVersion, Title = clientTitle}, ClientCapabilities.Default, name) { }

    public Agent(string cmd, string arguments, string workingDirectory, string clientName = "", string clientVersion = "1.0", string? clientTitle = null, string? name = null) :
        this(new AgentConnection(cmd, arguments, workingDirectory), clientName, clientVersion, clientTitle, name) { }    
    #endregion

    #region Methods
    public async Task<Result<InitializeResponse>> InitializeAsync(CancellationToken cancellationToken = default) =>
        await connection.InitializeAsync(new InitializeRequest { ClientCapabilities = clientCapabilities, ClientInfo = clientInfo, ProtocolVersion = 1 }, cancellationToken)
        .Map(Initialize);

    public async Task<Result<Session>> NewSessionAsync(string cwd, CancellationToken cancellationToken = default) => 
        await connection.NewSessionAsync(new NewSessionRequest() { Cwd = cwd }, cancellationToken)
        .Map(NewSession);

    public async Task<Result<PromptResponse>> PromptAsync(string sessionid, string prompt, CancellationToken cancellationToken = default) =>
        await connection.PromptAsync(new PromptRequest() { SessionId = sessionid, Prompt = { new ContentBlock() } }, cancellationToken);

    public Agent WithName(string name)
    {
        this.Name = name;
        return this;
    }

    public Agent WithConnectionTracing(SourceLevels sourceLevel, params TraceListener[] listeners)
    {
        this.connection.TraceLevel = sourceLevel;
        if (listeners != null)
        {
            foreach (var l in listeners)
            {
                this.connection.TraceListeners.Add(l);
            }
        }
        return this;
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    protected Session NewSession(NewSessionResponse r)
    {
        var s = new Session(this, r.SessionId, r);
        sessions.Add(r.SessionId, s);
        return s;
    }

    protected InitializeResponse Initialize(InitializeResponse r)
    {
        this.agentInitializeResponse = r;
        return r;
    }
    #endregion

    #region Properties
    public string? Name { get; private set; }

    public bool IsInitialized => agentInitializeResponse != null && agentInitializeResponse.ProtocolVersion  == 1;

    public AgentCapabilities AgentCapabilities => agentInitializeResponse?.AgentCapabilities ?? throw new AgentNotInitializedException();
    
    public AgentInfo AgentInfo => agentInitializeResponse?.AgentInfo ?? throw new AgentNotInitializedException();

    public ICollection<AuthMethod> AuthenticationMethods => agentInitializeResponse?.AuthMethods ?? throw new AgentNotInitializedException();

    #endregion

    #region Fields
    public readonly AgentConnection connection;
    protected readonly ClientCapabilities clientCapabilities;
    protected readonly Implementation clientInfo;
    protected InitializeResponse? agentInitializeResponse;
    public readonly Dictionary<string, Session> sessions = new Dictionary<string, Session>();
    protected ulong sessionCounter = 0;
    protected ulong terminalCounter = 0;
    #endregion   
}
