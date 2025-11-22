namespace OnlyHumans.Acp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
   
    // Agent interface: methods the Client can call on the Agent
    public interface IAgent
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
    }

    // Client interface: methods the Agent can call on the Client
    public interface IClient
    {
        Task SessionUpdateAsync(SessionNotification notification);
        Task<Result<RequestPermissionResponse>> RequestPermissionAsync(RequestPermissionRequest request);
            
        Task<Result<CreateTerminalResponse>> CreateTerminalAsync(CreateTerminalRequest request);
        Task<Result<KillTerminalCommandResponse>> KillTerminalCommandAsync(KillTerminalCommandRequest request);
        Task<Result<ReleaseTerminalResponse>> ReleaseTerminalAsync(ReleaseTerminalRequest request);
        Task<Result<TerminalOutputResponse>> TerminalOutputAsync(TerminalOutputRequest request);
        Task<Result<WaitForTerminalExitResponse>> WaitForTerminalExitAsync(WaitForTerminalExitRequest request);
        Task<Result<ReadTextFileResponse>> ReadTextFileAsync(ReadTextFileRequest request);
        Task<Result<WriteTextFileResponse>> WriteTextFileAsync(WriteTextFileRequest request);

        // Extension method for custom RPC calls
        Task<Result<Dictionary<string, object>>> ExtMethodAsync(string method, Dictionary<string, object>? parameters = null);
        // Extension notification for custom notifications
        Task ExtNotificationAsync(string notification, Dictionary<string, object>? parameters = null);
    }
}
