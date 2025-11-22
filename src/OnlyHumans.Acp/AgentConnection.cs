namespace OnlyHumans.Acp;

using Microsoft;
using Nerdbank.Streams;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static Result;

public class AgentConnection : Runtime, IDisposable, IAgent
{
    public AgentConnection(IClient client, string cmd, string? arguments = null, string? workingDirectory = null, TraceListener? traceListener = null, bool monitor = false)
    {
        this.client = client;
        this.cmdLine = cmd + " " + arguments;  
        psi = new ProcessStartInfo();
        psi.FileName = cmd;
        psi.Arguments = arguments;
        psi.WorkingDirectory = workingDirectory ?? AssemblyLocation;
        psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
        psi.StandardInputEncoding = System.Text.Encoding.UTF8;          
        psi.RedirectStandardInput = true;
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;
        process.Exited += (e, args) =>
        {
            Info("Agent subprocess {0} exited.", cmd);
        };
        process.Start();

        var ostream = process.StandardInput.BaseStream;
        var istream = process.StandardOutput.BaseStream;
        if (monitor)
        {
            var monitoringStream = new MonitoringStream(process.StandardOutput.BaseStream);
            var monitoringStream2 = new MonitoringStream(process.StandardInput.BaseStream);
            incomingData = new List<byte>();
            outgoingData = new List<byte>();

            monitoringStream.DidReadAny += (s, span) =>
            {
                incomingData.AddRange(span);
            };

            monitoringStream2.DidWriteAny += (s, span) =>
            {

                outgoingData.AddRange(span);
            };
            ostream = monitoringStream2;
            istream = monitoringStream;
            
        }

        jsonrpc = new JsonRpc(new NewLineDelimitedMessageHandler(ostream, istream, new JsonMessageFormatter()));
        jsonrpc.TraceSource.Switch.Level = SourceLevels.Verbose;    
        if (traceListener != null) jsonrpc.TraceSource.Listeners.Add(traceListener);

        // Register client methods                
        jsonrpc.AddLocalRpcMethod("fs/read_text_file", client.ReadTextFileAsync);
        jsonrpc.AddLocalRpcMethod("fs/write_text_file", client.WriteTextFileAsync);
        jsonrpc.AddLocalRpcMethod("session/request_permission", client.RequestPermissionAsync);
        jsonrpc.AddLocalRpcMethod("session/update", client.SessionUpdateAsync);
        jsonrpc.AddLocalRpcMethod("terminal/create", client.CreateTerminalAsync);
        jsonrpc.AddLocalRpcMethod("terminal/kill", client.KillTerminalCommandAsync);
        jsonrpc.AddLocalRpcMethod("terminal/output", client.TerminalOutputAsync);
        jsonrpc.AddLocalRpcMethod("terminal/release", client.ReleaseTerminalAsync);
        jsonrpc.AddLocalRpcMethod("terminal/wait_for_exit", client.WaitForTerminalExitAsync);
    
        jsonrpc.StartListening();               
    }

    #region Methods

    #region IAgent Implementation
    
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

    
    public void Stop()
    {
        process.Kill(); 
    }   
    public void Dispose()
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
            Error("Error disposing AgentConnection: {0}", ex.Message);
        }
        jsonrpc.Dispose();
        process.Dispose();
    }
    #endregion

    #region Fields
    public readonly IClient client;
    public readonly string cmdLine;
    protected readonly ProcessStartInfo psi;
    public readonly Process process;
    public List<byte>? incomingData;
    public List<byte>? outgoingData;
    protected JsonRpc jsonrpc;

    #endregion

   
}

