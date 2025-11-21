namespace OnlyHumans.Acp;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nerdbank.Streams;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using static Result;

public class AgentConnection : Runtime, IAgent, IJsonRpcMessageHandler
{
    public AgentConnection(IClient client, string cmd, string? arguments = null, string? workingDirectory = null, TraceListener? traceListener = null)
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
        
        /*
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug("Agent subprocess {0} output: {1}", cmd, e.Data);
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Error("Agent subprocess {0} error: {1}", cmd, e.Data);
            }
        };
          */     
        process.Start();
        jsonrpc = new JsonRpc(FullDuplexStream.Splice(process.StandardOutput.BaseStream, process.StandardInput.BaseStream));
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
    
    public async Task<Result<InitializeResponse>> InitializeAsync(InitializeRequest request)
       => await ExecuteAsync(jsonrpc.InvokeAsync<InitializeResponse>("initialize", request));

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

    #endregion

    #region Fields
    public readonly IClient client;
    public readonly string cmdLine;
    protected readonly ProcessStartInfo psi;
    protected readonly Process process;
    JsonRpc jsonrpc;
    #endregion

    // IJsonRpcMessageHandler implementation
    public  async ValueTask WriteAsync(JsonRpcMessage message, CancellationToken token)
    {
        Interlocked.Increment(ref busy);    
        //using var writer = new StreamWriter(process.StandardInput.BaseStream, psi.StandardInputEncoding, leaveOpen: true);
        await process.StandardInput.WriteLineAsync(JsonConvert.SerializeObject(message));
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();
        Interlocked.Decrement(ref busy);

    }

    public async ValueTask<JsonRpcMessage> ReadAsync(CancellationToken token)
    {
        Interlocked.Increment(ref busy);    
        //using var reader = new StreamReader(process.StandardOutput.BaseStream, psi.StandardOutputEncoding, leaveOpen: true);
        var s = await process.StandardOutput.ReadLineAsync();

        Interlocked.Decrement(ref busy);
        return JsonConvert.DeserializeObject<JsonRpcMessage>(s);
    }

    public bool CanRead => !process.HasExited && Interlocked.Read(ref busy) == 0 && process.StandardOutput.BaseStream.CanRead;

    public bool CanWrite => !process.HasExited && Interlocked.Read(ref busy) == 0 && process.StandardInput.BaseStream.CanWrite;

    public IJsonRpcMessageFormatter Formatter { get; } = new JsonMessageFormatter();

    public long busy = 0;
}

