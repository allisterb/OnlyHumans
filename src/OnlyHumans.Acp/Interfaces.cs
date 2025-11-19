namespace OnlyHumans.Acp;
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Interface for ACP Agent.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Handles initialization request from the client.
    /// </summary>
    /// <param name="request">Initialization request parameters.</param>
    /// <returns>Initialization response.</returns>
    Task<InitializeResponse> InitializeAsync(InitializeRequest request);

    /// <summary>
    /// Handles authentication request from the client.
    /// </summary>
    /// <param name="request">Authentication request parameters.</param>
    /// <returns>Authentication response.</returns>
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);

    /// <summary>
    /// Handles prompt request from the client.
    /// </summary>
    /// <param name="request">Prompt request parameters.</param>
    /// <returns>Prompt response.</returns>
    Task<PromptResponse> PromptAsync(PromptRequest request);

    /// <summary>
    /// Handles session loading request.
    /// </summary>
    /// <param name="request">Load session request parameters.</param>
    /// <returns>Load session response.</returns>
    Task<LoadSessionResponse> LoadSessionAsync(LoadSessionRequest request);

    /// <summary>
    /// Handles new session creation request.
    /// </summary>
    /// <param name="request">New session request parameters.</param>
    /// <returns>New session response.</returns>
    Task<NewSessionResponse> NewSessionAsync(NewSessionRequest request);

    /// <summary>
    /// Handles session mode change request.
    /// </summary>
    /// <param name="request">Set session mode request parameters.</param>
    /// <returns>Set session mode response.</returns>
    Task<SetSessionModeResponse> SetSessionModeAsync(SetSessionModeRequest request);

    /// <summary>
    /// Handles session model change request.
    /// </summary>
    /// <param name="request">Set session model request parameters.</param>
    /// <returns>Set session model response.</returns>
    Task<SetSessionModelResponse> SetSessionModelAsync(SetSessionModelRequest request);

    /// <summary>
    /// Handles permission request.
    /// </summary>
    /// <param name="request">Request permission parameters.</param>
    /// <returns>Request permission response.</returns>
    Task<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request);

    /// <summary>
    /// Handles cancel notification.
    /// </summary>
    /// <param name="notification">Cancel notification parameters.</param>
    /// <returns>Task representing the operation.</returns>
    Task CancelAsync(CancelNotification notification);
}

/// <summary>
/// Interface for ACP Client.
/// </summary>
public interface IClient
{
    Task<Result<RequestPermissionResponse>> RequestPermission(RequestPermissionRequest requestPermissionRequest);

    Task SessionUpdate(SessionNotification sessionNotification);

    Task<Result<WriteTextFileResponse>> WriteTextFile(WriteTextFileRequest request);
    /// <summary>
    /// Handles agent notification.
    /// </summary>
    /// <param name="notification">Agent notification parameters.</param>
    /// <returns>Task representing the operation.</returns>
    Task NotifyAsync(AgentNotification notification);

    /// <summary>
    /// Handles agent request.
    /// </summary>
    /// <param name="request">Agent request parameters.</param>
    /// <returns>Agent response.</returns>
    Task<AgentResponse> RequestAsync(AgentRequest request);

    /// <summary>
    /// Handles agent outgoing message.
    /// </summary>
    /// <param name="message">Agent outgoing message parameters.</param>
    /// <returns>Task representing the operation.</returns>
    Task SendMessageAsync(AgentOutgoingMessage message);
}
