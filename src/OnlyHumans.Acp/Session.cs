namespace OnlyHumans.Acp;

using System.Collections.Generic;
using System.Threading.Tasks;

public class Session : Runtime
{
    #region Constructors
    public Session(Client agent, string sessionId, object agentResponse)
    {
        this.agent = agent;
        this.sessionId = sessionId;
        this.agentResponse = agentResponse;
    }
    #endregion

    #region Properties
    public Role CurrentTurnRole => (turns.PeekIfNotEmpty() is null or PromptResponse) ? Role.User : Role.Assistant;

    public bool CurrentTurnIsTool => CurrentTurnRole == Role.Assistant && turns.Peek() is SessionUpdateToolCall or SessionUpdateToolCallUpdate;
    #endregion

    #region Methods
    public Task<Result<SetSessionModelResponse>> SetSessionModel(string modelId)
        => this.agent.connection.SetSessionModelAsync(new SetSessionModelRequest() { SessionId = sessionId, ModelId = modelId });
       
    public Task<Result<PromptResponse>> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        UpdateSessionState(request);
        return agent.connection.PromptAsync(request, cancellationToken)
        .Then(UpdateSessionState);
    }

    public Task<Result<PromptResponse>> PromptAsync(ContentBlock[] prompt, CancellationToken cancellationToken = default) =>
        PromptAsync(new PromptRequest() { SessionId = sessionId, Prompt = prompt }, cancellationToken);

    public Task<Result<PromptResponse>> PromptAsync(string prompt, CancellationToken cancellationToken = default) =>
       PromptAsync(new PromptRequest() { SessionId = sessionId, Prompt = [ContentBlock._Text(prompt)] }, cancellationToken);

    internal void UpdateSessionState(PromptRequest prompt)
    {
        turns.Push(prompt);
        Debug("Updating session state with user prompt {0}...", prompt.Message.Truncate(16));
    }

    internal PromptResponse UpdateSessionState(PromptResponse response)
    {
        turns.Push(response);
        Debug("Updating session state with agent prompt response {0}...", response.Message.Truncate(16));
        return response;    
    }

    internal void UpdateSessionState(SessionUpdate m)
    {        
        if (m is SessionUpdateAgentMessageChunk sx)
        {
            turns.Push(sx);
            Debug("Updating session state with agent response message {0}...", sx.Content.Message.Truncate(16));
        }
        else if (m is SessionUpdatePlan p)
        {
            turns.Push(p);
        }
        else if (m is SessionUpdateToolCall tc)
        {
            turns.Push(tc);
        }
        else if (m is SessionUpdateToolCallUpdate tcu)
        {
            turns.Push(tcu);
        }
    }
    #endregion

    #region Fields
    public readonly Client agent;
    public readonly string sessionId;
    public readonly object agentResponse;
    public string? model;
    public readonly Stack<ITurn> turns = new Stack<ITurn>();   
    #endregion
}

