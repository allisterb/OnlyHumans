namespace OnlyHumans.Acp;

using System.Diagnostics;

using static Result;

/// <summary>
/// A client that connects to an agent using the ACP JSON-RPC protocol.
/// </summary>
public class Client : Runtime, IDisposable
{
    #region Constructors
    public Client(AgentConnection agentConnection, string? agentName, Implementation clientInfo, ClientCapabilities clientCapabilities)
    {
        this.connection = agentConnection;
        this.clientInfo = clientInfo;
        this.clientCapabilities = clientCapabilities;
        this.AgentName = agentName;

        this.connection.SessionUpdateAsync += UpdateSessionState;
        this.connection.RequestPermissionAsync += RequestPermission;
        this.connection.CreateTerminalAsync += (req) => this.CreateTerminalAsync?.Invoke(req) ?? NotImplementedAsync<CreateTerminalResponse>();
        this.connection.KillTerminalCommandAsync += (req) => this.KillTerminalCommandAsync?.Invoke(req) ?? NotImplementedAsync<KillTerminalCommandResponse>();
        this.connection.ReleaseTerminalAsync += (req) => this.ReleaseTerminalAsync?.Invoke(req) ?? NotImplementedAsync<ReleaseTerminalResponse>();
        this.connection.TerminalOutputAsync += (req) => this.TerminalOutputAsync?.Invoke(req) ?? NotImplementedAsync<TerminalOutputResponse>();
        this.connection.WaitForTerminalExitAsync += (req) => this.WaitForTerminalExitAsync?.Invoke(req) ?? NotImplementedAsync<WaitForTerminalExitResponse>();
        this.connection.ReadTextFileAsync += (req) => this.ReadTextFileAsync?.Invoke(req) ?? NotImplementedAsync<ReadTextFileResponse>();
        this.connection.WriteTextFileAsync += (req) => this.WriteTextFileAsync?.Invoke(req) ?? NotImplementedAsync<WriteTextFileResponse>();
        this.connection.ClientExtMethodAsync += (method, dict) => this.ClientExtMethodAsync?.Invoke(method, dict) ?? NotImplementedAsync<Dictionary<string, object>>();
        this.connection.ClientExtNotificationAsync += (method, dict) => this.ClientExtNotificationAsync?.Invoke(method, dict) ?? NotImplementedAsync();
    }
    
    public Client(AgentConnection agentConnection, string? agentName = null, string clientName = "", string clientVersion = "1.0", string clientTitle="")
        : this(agentConnection, agentName, new Implementation() { Name = clientName, Version = clientVersion, Title = clientTitle}, ClientCapabilities.Default) { }

    public Client(string agentCmd, string agentCmdArgs, string agentCmdWorkingDirectory, IDictionary<string, string?>? agentCmdEnvVars = null, string? agentName = null, string clientName = "", string clientVersion = "1.0", string clientTitle = "") :
        this(new AgentConnection(agentCmd, agentCmdArgs, agentCmdWorkingDirectory, agentCmdEnvVars), agentName, clientName, clientVersion, clientTitle) { }    
    #endregion

    #region Methods
    public Task<Result<InitializeResponse>> InitializeAsync(CancellationToken cancellationToken = default) =>
        connection.InitializeAsync(new() { ClientCapabilities = clientCapabilities, ClientInfo = clientInfo, ProtocolVersion = 1 }, cancellationToken)
        .Then(Initialize);

    public Task<Result<AuthenticateResponse>> AuthenticateAsync(string methodId, Dictionary<string, object> properties) =>
        connection.AuthenticateAsync(new() { MethodId = methodId, _meta = properties });

    public Task<Result<Session>> NewSessionAsync(string cwd, CancellationToken cancellationToken = default) => 
        connection.NewSessionAsync(new() { Cwd = cwd }, cancellationToken).Then(NewSession);

    public Client WithAgentName(string name)
    {
        AgentName = name;
        return this;
    }

    /// <summary>
    /// Enable tracing for the JSON-RPC connection.
    /// </summary>
    /// <param name="sourceLevel">Trace level</param>
    /// <param name="listeners">Trace listeners to add</param>
    /// <returns></returns>
    public Client WithConnectionTracing(SourceLevels sourceLevel, params TraceListener[] listeners)
    {
        connection.TraceLevel = sourceLevel;
        if (listeners != null)
        {
            foreach (var l in listeners)
            {
                connection.TraceListeners.Add(l);
            }
        }
        return this;
    }

    public Client WithVerboseConsoleConnectionTracing() => WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener());
    
    public void Dispose()
    {
        connection.Dispose();
    }

    protected InitializeResponse Initialize(InitializeResponse r) => this.agentInitializeResponse = r;

    protected Session NewSession(NewSessionResponse r)
        => this.sessions[r.SessionId] = new Session(this, r.SessionId, r);

    #region Event handlers
    protected Task UpdateSessionState(SessionNotification notification)
    {
        var session = sessions[notification.SessionId];
        session.UpdateSessionState(notification.Update);
        return SessionUpdateAsync?.Invoke(notification) ?? Task.CompletedTask;
    }

    protected Task<RequestPermissionResponse> RequestPermission(RequestPermissionRequest request)
    {
        throw new NotImplementedException();
    }
    #endregion

    #endregion

    #region Properties
    public string? AgentName { get; private set; }

    public bool IsInitialized => agentInitializeResponse != null && agentInitializeResponse.ProtocolVersion  == 1;

    public AgentCapabilities AgentCapabilities => agentInitializeResponse?.AgentCapabilities ?? throw new AgentNotInitializedException();
    
    public AgentInfo AgentInfo => agentInitializeResponse?.AgentInfo ?? throw new AgentNotInitializedException();

    public ICollection<AuthMethod> AuthenticationMethods => agentInitializeResponse?.AuthMethods ?? throw new AgentNotInitializedException();

    #endregion

    #region Events
    public event ClientEventHandlerAsync<SessionNotification>? SessionUpdateAsync;
    public event ClientEventHandlerAsync<RequestPermissionRequest, RequestPermissionResponse>? RequestPermissionAsync;
    public event ClientEventHandlerAsync<CreateTerminalRequest, CreateTerminalResponse>? CreateTerminalAsync;
    public event ClientEventHandlerAsync<KillTerminalCommandRequest, KillTerminalCommandResponse>? KillTerminalCommandAsync;
    public event ClientEventHandlerAsync<ReleaseTerminalRequest, ReleaseTerminalResponse>? ReleaseTerminalAsync;
    public event ClientEventHandlerAsync<TerminalOutputRequest, TerminalOutputResponse>? TerminalOutputAsync;
    public event ClientEventHandlerAsync<WaitForTerminalExitRequest, WaitForTerminalExitResponse>? WaitForTerminalExitAsync;
    public event ClientEventHandlerAsync<ReadTextFileRequest, ReadTextFileResponse>? ReadTextFileAsync;
    public event ClientEventHandlerAsync<WriteTextFileRequest, WriteTextFileResponse>? WriteTextFileAsync;
    public event ClientEventHandlerAsync2<string, Dictionary<string, object>, Dictionary<string, object>>? ClientExtMethodAsync;
    public event ClientEventHandlerAsync2<string, Dictionary<string, object>>? ClientExtNotificationAsync;   
    #endregion

    #region Fields
    public readonly AgentConnection connection;
    protected readonly ClientCapabilities clientCapabilities;
    protected readonly Implementation clientInfo;
    protected InitializeResponse? agentInitializeResponse;
    public readonly Dictionary<string, Session> sessions = new Dictionary<string, Session>();
    #endregion   
}
