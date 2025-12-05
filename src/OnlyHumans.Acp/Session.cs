namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum SessionTurn
{
    Agent,
    User
}

public class Session : Runtime
{
    public Session(Agent agent, string sessionId, object agentResponse)
    {
        this.agent = agent;
        this.sessionId = sessionId;
        this.agentResponse = agentResponse;
    }

    public async Task<Result<SetSessionModelResponse>> SetSessionModel(SetSessionModelRequest request) => await this.agent.SetSessionModelAsync(request);

    public async Task<Result<PromptResponse>> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default) =>
       await agent.connection.PromptAsync(request, cancellationToken);

    public async Task<Result<PromptResponse>> PromptAsync(string prompt, CancellationToken cancellationToken = default) =>
       await PromptAsync(new PromptRequest() { SessionId = sessionId, Prompt = { ContentBlock._Text(prompt) } }, cancellationToken);

    internal void UpdateSessionState(SessionUpdate m)
    {
        updates.Push(m);
        currentTurn = (m is SessionUpdateAgentMessageChunk) ? SessionTurn.User : SessionTurn.Agent;    
    }
           
    #region Fields
    public readonly Agent agent;
    public readonly string sessionId;
    public readonly object agentResponse;
    public string? model;
    public Stack<SessionUpdate> updates = new Stack<SessionUpdate>();   
    public SessionTurn currentTurn = SessionTurn.User;
    #endregion

    #region Events
    #endregion
}

