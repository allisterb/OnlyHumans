namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Session : Runtime
{
    public Session(Agent agent, string sessionId, object agentResponse)
    {
        this.agent = agent;
        this.sessionId = sessionId;
        this.agentResponse = agentResponse;
    }

    #region Methods
    public async Task<Result<SetSessionModelResponse>> SetSessionModel(string modelId)
        => await this.agent.SetSessionModelAsync(new SetSessionModelRequest() { SessionId = sessionId, ModelId = modelId });
       
    public async Task<Result<PromptResponse>> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        var r = await agent.connection.PromptAsync(request, cancellationToken);
        if (r.IsSuccess && r.Value.StopReason == StopReason.EndTurn)
        {
            UpdateSessionState(request);
        }
        return r;
    }
        
    public async Task<Result<PromptResponse>> PromptAsync(string prompt, CancellationToken cancellationToken = default) =>
       await PromptAsync(new PromptRequest() { SessionId = sessionId, Prompt = { ContentBlock._Text(prompt) } }, cancellationToken);


    internal void UpdateSessionState(PromptRequest prompt)
    {
        updates.Push(prompt);
        currentTurn = SessionTurn.Agent;
    }

    internal void UpdateSessionState(SessionUpdate m)
    {        
        if (m is SessionUpdateAgentMessageChunk sx)
        {
            updates.Push(sx.Content);
            currentTurn = SessionTurn.User;
        }
        else if (m is SessionUpdatePlan p)
        {
            updates.Push(p);
            currentTurn = SessionTurn.User;
        }            
    }
    #endregion

    #region Fields
    public readonly Agent agent;
    public readonly string sessionId;
    public readonly object agentResponse;
    public string? model;
    public readonly Stack<IContentBlock> updates = new Stack<IContentBlock>();   
    public SessionTurn currentTurn = SessionTurn.User;
    #endregion

    #region Types
    public enum SessionTurn
    {
        Agent,
        User
    }
    #endregion
}

