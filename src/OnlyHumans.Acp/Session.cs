namespace OnlyHumans.Acp;

using System.Collections.Generic;
using System.Threading.Tasks;

public class Session : Runtime
{
    #region Constructors
    public Session(Client client, string sessionId, NewSessionResponse agentResponse)
    {
        this.client = client;
        this.agentResponse = agentResponse;
        this.sessionId = sessionId;        
    }
    #endregion

    #region Properties
    public Role CurrentTurn => (turns.Count == 0 || turns.LastItem() is PromptResponse) ? Role.User : Role.Assistant;    
    
    public PromptRequest LastPrompt => prompts.IsNotEmpty() ? prompts.LastItem() : throw new InvalidOperationException("No user prompts have been made.");

    #endregion

    #region Methods
    public Task<Result<PromptResponse>> PromptAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        UpdateSessionState(request);
        return client.connection.PromptAsync(request, cancellationToken).Then(UpdateSessionState);
    }

    public Task<Result<PromptResponse>> PromptAsync(ContentBlock[] prompt, CancellationToken cancellationToken = default) =>
        PromptAsync(new PromptRequest() { SessionId = sessionId, Prompt = prompt }, cancellationToken);

    public Task<Result<PromptResponse>> PromptAsync(string prompt, CancellationToken cancellationToken = default) =>
       PromptAsync(new PromptRequest() { SessionId = sessionId, Prompt = [ContentBlock._Text(prompt)] }, cancellationToken);

    internal void UpdateSessionState(PromptRequest prompt)
    {
        prompts.Add(prompt);
        turns.Add(prompt);     
        Debug("Updating session state with user prompt: {0}...", prompt.Message.Truncate(16));
    }

    internal PromptResponse UpdateSessionState(PromptResponse response)
    {
        var prompt = prompts.LastItem();
        prompt.Response = response;
        response.updates = prompt.responseUpdates!;
        prompt.responseUpdates = null;
        turns.Add(response);
        promptResponses.Add(response);
        Debug("Updating session state with agent prompt response: {0} and {1} agent updates.", response.StopReason, response.updates?.Count ?? 0);
        return response;    
    }

    internal void UpdateSessionState(SessionUpdate m)
    {
        var prompt = prompts.LastItem();
        if (prompt.responseUpdates is null)
        {
            prompt.responseUpdates = new List<SessionUpdate>();
        }        
        prompt.responseUpdates.Add(m); 
        if (m is SessionUpdateAgentMessageChunk sx)
        {            
            Debug("Updating session state with agent message: {0}...", sx.Content.Message.Truncate(16));
        }
        else if (m is SessionUpdatePlan p)
        {
            Debug("Updating session state with agent plan: {0}...", p.Message.Truncate(16));
        }
        else if (m is SessionUpdateToolCall tc)
        {
            Debug("Updating session state with agent tool call: {0}...", tc.Message.Truncate(16));
        }
        else if (m is SessionUpdateToolCallUpdate tcu)
        {
            Debug("Updating session state with agent tool call update: {0}...", tcu.Message.Truncate(16));
        }
    }
    #endregion

    #region Fields
    public readonly Client client;
    public readonly NewSessionResponse agentResponse;
    public readonly string sessionId;    
    public string? model;
    public readonly List<ITurn> turns = new List<ITurn>();
    public readonly List<PromptRequest> prompts = new List<PromptRequest>();
    public readonly List<PromptResponse> promptResponses = new List<PromptResponse>();
    #endregion
}

