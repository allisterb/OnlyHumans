namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            StandardOutputEncoding = Encoding.UTF8,
            StandardInputEncoding = Encoding.UTF8,
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

        jsonrpc = new JsonRpc(new NewLineDelimitedMessageHandler(ostream, istream, new JsonMessageFormatter()));
        jsonrpc.TraceSource.Switch.Level = traceLevel;    
        if (traceListener != null) jsonrpc.TraceSource.Listeners.Add(traceListener);
        // Register client methods
        jsonrpc.AddLocalRpcMethod("fs/read_text_file", ClientSessionUpdateAsync);        
        jsonrpc.AddLocalRpcMethod("fs/write_text_file", ClientWriteTextFileAsync);
        jsonrpc.AddLocalRpcMethod("session/request_permission", ClientRequestPermissionAsync);
        jsonrpc.AddLocalRpcMethod("session/update", ClientSessionUpdateAsync);
        jsonrpc.AddLocalRpcMethod("terminal/create", ClientCreateTerminalAsync);
        jsonrpc.AddLocalRpcMethod("terminal/kill", ClientKillTerminalCommandAsync);
        jsonrpc.AddLocalRpcMethod("terminal/output", ClientTerminalOutputAsync);
        jsonrpc.AddLocalRpcMethod("terminal/release",ClientReleaseTerminalAsync);
        jsonrpc.AddLocalRpcMethod("terminal/wait_for_exit", ClientWaitForTerminalExitAsync);    
        
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

    #region Client Methods
    public Task ClientSessionUpdateAsync(SessionNotification request)
        => SessionUpdateAsync?.Invoke(request) ?? NotImplementedAsync();

    public Task<RequestPermissionResponse> ClientRequestPermissionAsync(RequestPermissionRequest request)
        => RequestPermissionAsync?.Invoke(request) ?? NotImplementedAsync<RequestPermissionResponse>();

    public Task<CreateTerminalResponse> ClientCreateTerminalAsync(CreateTerminalRequest request)
        => CreateTerminalAsync?.Invoke(request) ?? NotImplementedAsync<CreateTerminalResponse>();

    public Task<KillTerminalCommandResponse> ClientKillTerminalCommandAsync(KillTerminalCommandRequest request)
        => KillTerminalCommandAsync?.Invoke(request) ?? NotImplementedAsync<KillTerminalCommandResponse>();

    public Task<ReleaseTerminalResponse> ClientReleaseTerminalAsync(ReleaseTerminalRequest request)
        => ReleaseTerminalAsync?.Invoke(request) ?? NotImplementedAsync<ReleaseTerminalResponse>();

    public Task<TerminalOutputResponse> ClientTerminalOutputAsync(TerminalOutputRequest request)
        => TerminalOutputAsync?.Invoke(request) ?? NotImplementedAsync<TerminalOutputResponse>();

    public Task<WaitForTerminalExitResponse> ClientWaitForTerminalExitAsync(WaitForTerminalExitRequest request)
        => WaitForTerminalExitAsync?.Invoke(request) ?? NotImplementedAsync<WaitForTerminalExitResponse>();

    public Task<ReadTextFileResponse> ClientReadTextFileAsync(ReadTextFileRequest request)
        => ReadTextFileAsync?.Invoke(request) ?? NotImplementedAsync<ReadTextFileResponse>( );

    public Task<WriteTextFileResponse> ClientWriteTextFileAsync(WriteTextFileRequest request)
        => WriteTextFileAsync?.Invoke(request) ?? NotImplementedAsync<WriteTextFileResponse>();

    public Task<Dictionary<string, object>> _ClientExtMethodAsync(string method, Dictionary<string, object> parameters)
        => ClientExtMethodAsync?.Invoke(method, parameters) ?? NotImplementedAsync<Dictionary<string, object>>();

    public Task _ClientExtNotificationAsync(string method, Dictionary<string, object> parameters)
        => ClientExtNotificationAsync?.Invoke(method, parameters) ?? NotImplementedAsync();
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
   
    public readonly string cmdLine;
    public readonly StringBuilder? incomingData;
    public readonly StringBuilder? outgoingData;
    #endregion

    #region Events
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
    public event ClientEventHandlerAsync<SessionNotification>? SessionUpdateAsync;
    #endregion
}

