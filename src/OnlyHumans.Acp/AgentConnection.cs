namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Nerdbank.Streams;
using StreamJsonRpc;

using static Result;

public class AgentConnection : Runtime, IDisposable, IAgentConnection
{
    #region Constructors
    public AgentConnection(string cmd, string? arguments = null, string? workingDirectory = null, IDictionary<string, string?>? environmentVariables = null, SourceLevels traceLevel = SourceLevels.Information, TraceListener? traceListener = null, bool monitorIO = false)
    {                
        this.cmdLine = cmd + " " + arguments;
        psi = new ProcessStartInfo()
        {
            FileName = cmd,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? AssemblyLocation,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardInputEncoding = new UTF8Encoding(false),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (environmentVariables is not null)
        {
            foreach (var kv in environmentVariables)
            {                
                psi.EnvironmentVariables[kv.Key] = kv.Value;                
            }
        }
        process = new Process()
        {
            StartInfo = psi,
            EnableRaisingEvents = true,
        };
        process.Exited += (e, args) =>
        {
            Info("Agent process {0} exited.", cmd);
        };
        process.Start();
        
        var istream = process.StandardOutput.BaseStream;
        var ostream = process.StandardInput.BaseStream;        
        if (monitorIO)
        {
            var imonitoringStream = new MonitoringStream(process.StandardOutput.BaseStream);
            var omonitoringStream = new MonitoringStream(process.StandardInput.BaseStream);            
            incomingData = new StringBuilder();
            outgoingData = new StringBuilder();
            imonitoringStream.DidReadAny += (s, span) =>
            {
                incomingData.Append(Encoding.UTF8.GetString(span));
            };

            omonitoringStream.DidWriteAny += (s, span) =>
            {
                outgoingData.Append(Encoding.UTF8.GetString(span));
            };
            istream = imonitoringStream;
            ostream = omonitoringStream;                        
        }
        agentRpcEvents = new RpcEvents(this);
        jsonrpc = new JsonRpc(new JsonRpcMessageHandler(ostream, istream, new JsonMessageFormatter(), JsonRpcMessageHandler.DelimiterType.NewLine, false));
        jsonrpc.TraceSource.Switch.Level = traceLevel;    
        if (traceListener != null) jsonrpc.TraceSource.Listeners.Add(traceListener);
        jsonrpc.AddLocalRpcTarget(agentRpcEvents, new JsonRpcTargetOptions() { UseSingleObjectParameterDeserialization = true });             
        jsonrpc.StartListening();
    }
    #endregion
 
    #region Methods

    #region Agent Methods
    public async Task<Result<InitializeResponse>> InitializeAsync(InitializeRequest request, CancellationToken cancellationToken = default)
       => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<InitializeResponse>("initialize", request, cancellationToken));

    public async Task<Result<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<AuthenticateResponse>("authenticate", request, cancellationToken));

    public async Task<Result<NewSessionResponse>> NewSessionAsync(NewSessionRequest request, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<NewSessionResponse>("session/new", request, cancellationToken));

    public async Task<Result<LoadSessionResponse>> LoadSessionAsync(LoadSessionRequest request, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<LoadSessionResponse>("session/load", request, cancellationToken));

    public async Task<Result<PromptResponse>> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<PromptResponse>("session/prompt", request, cancellationToken));

    public async Task<Result<SetSessionModeResponse>> SetSessionModeAsync(SetSessionModeRequest request, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<SetSessionModeResponse>("session/set_mode", request, cancellationToken));

    public async Task<Result<SetSessionModelResponse>> SetSessionModelAsync(SetSessionModelRequest request, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<SetSessionModelResponse>("session/set_model", request, cancellationToken));

    public async Task CancelNotificationAsync(CancelNotification notification)
        => await jsonrpc.NotifyAsync("session/cancel", notification);

    public async Task<Result<Dictionary<string, object>>> ExtMethodAsync(string method, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<Dictionary<string, object>>(method, parameters, cancellationToken));

    public async Task<Result<None>> ExtNotificationAsync(string notification, Dictionary<string, object>? parameters = null)
        => await ExecuteAsync(jsonrpc.NotifyAsync(notification, parameters));
    #endregion

    public void Stop()
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        catch (Exception ex)
        {
            Error("Error killing agent sub-process {0}: {1}.", cmdLine, ex.Message);
        }
    }   

    public void Dispose()
    {
        Stop(); 
        jsonrpc.Dispose();
        process.Dispose();
    }
    #endregion

    #region Properties
    public SourceLevels TraceLevel
    {
        get => jsonrpc.TraceSource.Switch.Level;
        set => jsonrpc.TraceSource.Switch.Level = value;
    }

    public TraceListenerCollection TraceListeners => jsonrpc.TraceSource.Listeners;
    #endregion

    #region Fields
    protected readonly ProcessStartInfo psi;
    protected readonly Process process;
    protected readonly JsonRpc jsonrpc;
    protected readonly RpcEvents agentRpcEvents;
   
    public readonly string cmdLine;
    public readonly StringBuilder? incomingData;
    public readonly StringBuilder? outgoingData;
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

    #region Types
    public class RpcEvents
    {
        public RpcEvents(AgentConnection conn)
        {
            this.conn = conn;
        }

        [JsonRpcMethod("session/update", UseSingleObjectParameterDeserialization = true)]
        public Task SessionUpdateAsync(SessionNotification request) => conn.SessionUpdateAsync?.Invoke(request) ?? NotImplementedAsync();

        [JsonRpcMethod("session/request_permission", UseSingleObjectParameterDeserialization = true)]
        public Task<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request)
            => conn.RequestPermissionAsync?.Invoke(request) ?? NotImplementedAsync<RequestPermissionResponse>();

        [JsonRpcMethod("terminal/create", UseSingleObjectParameterDeserialization = true)]
        public Task<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request)
            => conn.CreateTerminalAsync?.Invoke(request) ?? NotImplementedAsync<CreateTerminalResponse>();

        [JsonRpcMethod("terminal/kill", UseSingleObjectParameterDeserialization = true)]
        public Task<KillTerminalCommandResponse> KillTerminalCommandAsync(KillTerminalCommandRequest request)
            => conn.KillTerminalCommandAsync?.Invoke(request) ?? NotImplementedAsync<KillTerminalCommandResponse>();

        [JsonRpcMethod("terminal/release", UseSingleObjectParameterDeserialization = true)]
        public Task<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request)
            => conn.ReleaseTerminalAsync?.Invoke(request) ?? NotImplementedAsync<ReleaseTerminalResponse>();

        [JsonRpcMethod("terminal/output", UseSingleObjectParameterDeserialization = true)]
        public Task<TerminalOutputResponse> TerminalOutputAsync(TerminalOutputRequest request)
            => conn.TerminalOutputAsync?.Invoke(request) ?? NotImplementedAsync<TerminalOutputResponse>();

        [JsonRpcMethod("terminal/wait_for_exit", UseSingleObjectParameterDeserialization = true)]
        public Task<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request)
            => conn.WaitForTerminalExitAsync?.Invoke(request) ?? NotImplementedAsync<WaitForTerminalExitResponse>();

        [JsonRpcMethod("fs/read_text_file", UseSingleObjectParameterDeserialization = true)]
        public Task<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request)
            => conn.ReadTextFileAsync?.Invoke(request) ?? NotImplementedAsync<ReadTextFileResponse>();

        [JsonRpcMethod("fs/write_text_file", UseSingleObjectParameterDeserialization = true)]
        public Task<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request)
            => conn.WriteTextFileAsync?.Invoke(request) ?? NotImplementedAsync<WriteTextFileResponse>();

        AgentConnection conn;
    }
    #endregion
}

