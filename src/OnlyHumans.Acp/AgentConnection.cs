namespace OnlyHumans.Acp;

using Nerdbank.Streams;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static Result;

public class AgentConnection : Runtime, IDisposable, IAgentConnection
{
    #region Constructors
    public AgentConnection(string cmd, string? arguments = null, string? workingDirectory = null, SourceLevels traceLevel = SourceLevels.Information, TraceListener? traceListener = null, bool monitorIO = false)
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
        process = new Process()
        {
            StartInfo = psi,
            EnableRaisingEvents = true,
        };
        process.Exited += (e, args) =>
        {
            Info("Agent subprocess {0} exited.", cmd);
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
        jsonrpc.AddLocalRpcMethod("fs/read_text_file", ClientReadTextFileAsync);        
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

    #region Events
    public event ClientEventHandlerAsync<RequestPermissionRequest, RequestPermissionResponse>? RequestPermissionAsync;
    public event ClientEventHandlerAsync<CreateTerminalRequest, CreateTerminalResponse>? CreateTerminalAsync;
    public event ClientEventHandlerAsync<KillTerminalCommandRequest, KillTerminalCommandResponse>? KillTerminalCommandAsync;
    public event ClientEventHandlerAsync<ReleaseTerminalRequest, ReleaseTerminalResponse>? ReleaseTerminalAsync;
    public event ClientEventHandlerAsync<TerminalOutputRequest, TerminalOutputResponse>? TerminalOutputAsync;
    public event ClientEventHandlerAsync<WaitForTerminalExitRequest, WaitForTerminalExitResponse>? WaitForTerminalExitAsync;
    public event ClientEventHandlerAsync<ReadTextFileRequest, ReadTextFileResponse>? ReadTextFileAsync;
    public event ClientEventHandlerAsync<WriteTextFileRequest, WriteTextFileResponse>? WriteTextFileAsync;
    public event ClientEventHandlerAsync<Dictionary<string, object>, Dictionary<string, object>>? ClientExtMethodAsync;
    public event ClientEventHandlerAsync<Dictionary<string, object>, Dictionary<string, object>>? ClientExtNotificationAsync;
    public event ClientEventHandlerAsync<SessionNotification>? SessionUpdateAsync;
    #endregion

    #region Methods

    #region Agent Methods
    public async Task<Result<InitializeResponse>> InitializeAsync(InitializeRequest request, CancellationToken cancellationToken = default)
       => await ExecuteAsync(jsonrpc.InvokeWithParameterObjectAsync<InitializeResponse>("initialize", request, cancellationToken));

    public async Task<Result<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request)
        => await ExecuteAsync(jsonrpc.InvokeAsync<AuthenticateResponse>("authenticate", request));

    public async Task<Result<NewSessionResponse>> NewSessionAsync(NewSessionRequest request)
        => await ExecuteAsync(jsonrpc.InvokeAsync<NewSessionResponse>("session/new", request));

    public async Task<Result<LoadSessionResponse>> LoadSessionAsync(LoadSessionRequest request)
        => await ExecuteAsync(jsonrpc.InvokeAsync<LoadSessionResponse>("session/load", request));

    public async Task<Result<PromptResponse>> PromptAsync(PromptRequest request)
        => await ExecuteAsync(jsonrpc.InvokeAsync<PromptResponse>("session/prompt", request));

    public async Task<Result<SetSessionModeResponse>> SetSessionModeAsync(SetSessionModeRequest request)
        => await ExecuteAsync(jsonrpc.InvokeAsync<SetSessionModeResponse>("session/set_mode", request));

    public async Task<Result<SetSessionModelResponse>> SetSessionModelAsync(SetSessionModelRequest request)
        => await ExecuteAsync(jsonrpc.InvokeAsync<SetSessionModelResponse>("session/set_model", request));

    public async Task CancelNotificationAsync(CancelNotification notification)
        => await jsonrpc.NotifyAsync("session/cancel", notification);

    public async Task<Result<Dictionary<string, object>>> ExtMethodAsync(string method, Dictionary<string, object>? parameters = null)
        => await ExecuteAsync(jsonrpc.InvokeAsync<Dictionary<string, object>>(method, parameters));

    public async Task ExtNotificationAsync(string notification, Dictionary<string, object>? parameters = null)
        => await jsonrpc.NotifyAsync(notification, parameters);
    #endregion

    #region Client Methods

    public Task ClientSessionUpdateAsync(SessionNotification request)
        => SessionUpdateAsync?.Invoke(this, new ClientEventArgs<SessionNotification>("SessionUpdate", request)) ?? Task.FromException(new NotImplementedException());

    public Task<RequestPermissionResponse> ClientRequestPermissionAsync(RequestPermissionRequest request)
        => RequestPermissionAsync?.Invoke(this, new ClientEventArgs<RequestPermissionRequest>("RequestPermission", request)) ?? Task.FromException<RequestPermissionResponse>(new NotImplementedException());

    public Task<CreateTerminalResponse> ClientCreateTerminalAsync(CreateTerminalRequest request)
        => CreateTerminalAsync?.Invoke(this, new ClientEventArgs<CreateTerminalRequest>("CreateTerminal", request)) ?? Task.FromException<CreateTerminalResponse>(new NotImplementedException());

    public Task<KillTerminalCommandResponse> ClientKillTerminalCommandAsync(KillTerminalCommandRequest request)
        => KillTerminalCommandAsync?.Invoke(this, new ClientEventArgs<KillTerminalCommandRequest>("KillTerminalCommand", request)) ?? Task.FromException<KillTerminalCommandResponse>(new NotImplementedException());

    public Task<ReleaseTerminalResponse> ClientReleaseTerminalAsync(ReleaseTerminalRequest request)
        => ReleaseTerminalAsync?.Invoke(this, new ClientEventArgs<ReleaseTerminalRequest>("ReleaseTerminal", request)) ?? Task.FromException<ReleaseTerminalResponse>(new NotImplementedException());

    public Task<TerminalOutputResponse> ClientTerminalOutputAsync(TerminalOutputRequest request)
        => TerminalOutputAsync?.Invoke(this, new ClientEventArgs<TerminalOutputRequest>("TerminalOutput", request)) ?? Task.FromException<TerminalOutputResponse>(new NotImplementedException());

    public Task<WaitForTerminalExitResponse> ClientWaitForTerminalExitAsync(WaitForTerminalExitRequest request)
        => WaitForTerminalExitAsync?.Invoke(this, new ClientEventArgs<WaitForTerminalExitRequest>("WaitForTerminalExit", request)) ?? Task.FromException<WaitForTerminalExitResponse>(new NotImplementedException());

    public Task<ReadTextFileResponse> ClientReadTextFileAsync(ReadTextFileRequest request)
        => ReadTextFileAsync?.Invoke(this, new ClientEventArgs<ReadTextFileRequest>("ReadTextFile2", request)) ?? Task.FromException<ReadTextFileResponse>(new NotImplementedException());

    public Task<WriteTextFileResponse> ClientWriteTextFileAsync(WriteTextFileRequest request)
        => WriteTextFileAsync?.Invoke(this, new ClientEventArgs<WriteTextFileRequest>("WriteTextFile", request)) ?? Task.FromException<WriteTextFileResponse>(new NotImplementedException());

    public Task<Dictionary<string, object>> ClientExtMethodAsync(Dictionary<string, object> parameters)
        => ClientExtMethodAsync?.Invoke(this, new ClientEventArgs<Dictionary<string, object>>("ClientExtMethod", parameters)) ?? Task.FromException<Dictionary<string, object>>(new NotImplementedException());

    public Task<Dictionary<string, object>> ClientExtNotificationAsync(Dictionary<string, object> parameters)
        => ClientExtNotificationAsync?.Invoke(this, new ClientEventArgs<Dictionary<string, object>>("ClientExtNotification", parameters)) ?? Task.FromException<Dictionary<string, object>>(new NotImplementedException());

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
            Error("Error killing process {0}: {1}.", cmdLine, ex.Message);
        }
    }   

    public void Dispose()
    {
        Stop(); 
        jsonrpc.Dispose();
        process.Dispose();
    }
    #endregion

    #region Fields
    protected readonly ProcessStartInfo psi;
    protected readonly Process process;
    protected readonly JsonRpc jsonrpc;
   
    public readonly string cmdLine;
    public readonly StringBuilder? incomingData;
    public readonly StringBuilder? outgoingData;
    #endregion

    

}

