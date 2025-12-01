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

    public async Task<Result<SetSessionModelResponse>> SetSessionModel(SetSessionModelRequest request) => await this.agent.SetSessionModelAsync(request);
    #region Fields
    public readonly Agent agent;
    public readonly string sessionId;
    public readonly object agentResponse;
    public string? model;
    #endregion
}

