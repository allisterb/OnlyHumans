namespace OnlyHumans.Acp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ClientEventArgs<T> : EventArgs
    {
        public ClientEventArgs(string eventName, T eventData)
        {
            this.eventName = eventName;
            this.eventData = eventData;
        }
        public readonly string eventName;
        public readonly T eventData;
    }
   
    public delegate Task ClientEventHandlerAsync<T>(object sender, ClientEventArgs<T> e);    

    public delegate Task<U> ClientEventHandlerAsync<T, U>(object sender, ClientEventArgs<T> e);

    // Agent connection interface: methods the client can call on the agent and events the agent can raise to the client
    public interface IAgentConnection
    {
        Task<Result<InitializeResponse>> InitializeAsync(InitializeRequest request, CancellationToken cancellationToken);
        Task<Result<AuthenticateResponse>> AuthenticateAsync(AuthenticateRequest request);
        Task<Result<NewSessionResponse>> NewSessionAsync(NewSessionRequest request);
        Task<Result<LoadSessionResponse>> LoadSessionAsync(LoadSessionRequest request);
        Task<Result<PromptResponse>> PromptAsync(PromptRequest request);
        Task<Result<SetSessionModeResponse>> SetSessionModeAsync(SetSessionModeRequest request);
        Task<Result<SetSessionModelResponse>> SetSessionModelAsync(SetSessionModelRequest request);
        Task CancelNotificationAsync(CancelNotification notification);
        // Extension method for custom RPC calls
        Task<Result<Dictionary<string, object>>> ExtMethodAsync(string method, Dictionary<string, object>? parameters = null);
        // Extension notification for custom notifications
        Task ExtNotificationAsync(string notification, Dictionary<string, object>? parameters = null);

        event ClientEventHandlerAsync<SessionNotification> SessionUpdateAsync;
        event ClientEventHandlerAsync<RequestPermissionRequest, RequestPermissionResponse> RequestPermissionAsync;
        event ClientEventHandlerAsync<CreateTerminalRequest, CreateTerminalResponse> CreateTerminalAsync;
        event ClientEventHandlerAsync<KillTerminalCommandRequest, KillTerminalCommandResponse> KillTerminalCommandAsync;
        event ClientEventHandlerAsync<ReleaseTerminalRequest, ReleaseTerminalResponse> ReleaseTerminalAsync;
        event ClientEventHandlerAsync<TerminalOutputRequest, TerminalOutputResponse> TerminalOutputAsync;
        event ClientEventHandlerAsync<WaitForTerminalExitRequest, WaitForTerminalExitResponse> WaitForTerminalExitAsync;
        event ClientEventHandlerAsync<WriteTextFileRequest, WriteTextFileResponse> WriteTextFileAsync;
        // Extension method for custom RPC calls
        event ClientEventHandlerAsync<Dictionary<string, object>, Dictionary<string, object>> ClientExtMethodAsync;
        // Extension notification for custom notifications
        event ClientEventHandlerAsync<Dictionary<string, object>, Dictionary<string, object>> ClientExtNotificationAsync;

    }

}
