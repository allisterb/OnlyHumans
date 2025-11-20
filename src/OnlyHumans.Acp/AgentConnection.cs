namespace OnlyHumans.Acp;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

using static Result;

public class AgentConnection : Runtime, IAgent
{
    public AgentConnection(IClient client, string cmd, string? workingDirectory = null, params string[] arguments)
    {
        this.client = client;
        this.cmdLine = cmd + " " + arguments.JoinWith(" ");  
        psi = new ProcessStartInfo();
        psi.FileName = cmd;        
        psi.Arguments = arguments.JoinWith(" ");    
        psi.WorkingDirectory = workingDirectory ?? AssemblyLocation;
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
        process.Start();
        jsonrpc = new JsonRpc(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
        // Register client methods from meta.json
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
    IClient client;
    public string cmdLine;
    protected ProcessStartInfo psi;
    protected Process process;
    JsonRpc jsonrpc;
    #endregion
}

