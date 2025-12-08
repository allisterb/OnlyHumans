namespace OnlyHumans.Acp;

using System.Collections.Generic;
using System.Threading.Tasks;

public delegate Task ClientEventHandlerAsync<T>(T e);    

public delegate Task<U> ClientEventHandlerAsync<T, U>(T e);

public delegate Task ClientEventHandlerAsync2<T, U>(T e1, U e2);

public delegate Task<V> ClientEventHandlerAsync2<T, U, V>(T e1, U e2);

/// <summary>
/// Methods the client can call agent-side.
/// </summary>
public interface IAgent
{
    Task<Result<InitializeResponse>> InitializeAsync(InitializeRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken = default);
    Task<Result<NewSessionResponse>> NewSessionAsync(NewSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoadSessionResponse>> LoadSessionAsync(LoadSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<PromptResponse>> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default);
    Task<Result<SetSessionModeResponse>> SetSessionModeAsync(SetSessionModeRequest request, CancellationToken cancellationToken = default);
    Task<Result<SetSessionModelResponse>> SetSessionModelAsync(SetSessionModelRequest request, CancellationToken cancellationToken = default);
    Task CancelNotificationAsync(CancelNotification notification);
    // Extension method for custom RPC calls
    Task<Result<Dictionary<string, object>>> ExtMethodAsync(string method, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    // Extension notification for custom notifications
    Task<Result<None>> ExtNotificationAsync(string notification, Dictionary<string, object>? parameters = null);   
}

/// <summary>
/// Events the agent can raise client-side.
/// </summary>
public interface IClientEvents
{
    event ClientEventHandlerAsync<SessionNotification> SessionUpdateAsync;
    event ClientEventHandlerAsync<RequestPermissionRequest, RequestPermissionResponse> RequestPermissionAsync;
    event ClientEventHandlerAsync<CreateTerminalRequest, CreateTerminalResponse> CreateTerminalAsync;
    event ClientEventHandlerAsync<KillTerminalCommandRequest, KillTerminalCommandResponse> KillTerminalCommandAsync;
    event ClientEventHandlerAsync<ReleaseTerminalRequest, ReleaseTerminalResponse> ReleaseTerminalAsync;
    event ClientEventHandlerAsync<TerminalOutputRequest, TerminalOutputResponse> TerminalOutputAsync;
    event ClientEventHandlerAsync<WaitForTerminalExitRequest, WaitForTerminalExitResponse> WaitForTerminalExitAsync;
    event ClientEventHandlerAsync<WriteTextFileRequest, WriteTextFileResponse> WriteTextFileAsync;
    // Extension method for custom RPC calls
    event ClientEventHandlerAsync2<string, Dictionary<string, object>, Dictionary<string, object>> ClientExtMethodAsync;
    // Extension notification for custom notifications
    event ClientEventHandlerAsync2<string, Dictionary<string, object>> ClientExtNotificationAsync;
}

/// <summary>
/// A message in a conversation between a user and an assistant.
/// </summary>
public interface IMessage
{
    string Message { get; }
}
/// <summary>
/// A turn of a conversation between a user and an assistant.
/// </summary>
public interface ITurn : IMessage
{
    Role Role { get; }      
}

public class AgentNotInitializedException : InvalidOperationException
{
    public AgentNotInitializedException() : base("Agent not initialized. Call InitializeAsync first.") { }
}
