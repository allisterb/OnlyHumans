using LLama;
using LLama.Common;
using LLama.Extensions;
using LLama.Sampling;
using LLamaSharp.SemanticKernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SKChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

namespace OnlyHumans.ChatCompletion;

public class LlamaFunctionCallingChatCompletionService : IChatCompletionService
{
    private readonly InteractiveExecutor _executor;

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public LlamaFunctionCallingChatCompletionService(InteractiveExecutor executor)
    {
        _executor = executor;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        SKChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var settings = executionSettings as LLamaSharpPromptExecutionSettings;
        List<KernelFunction>? functions = null;
        if (executionSettings?.FunctionChoiceBehavior != null && kernel != null)
        {
             functions = kernel.Plugins.SelectMany(p => p).ToList();
        }

        string prompt = FunctionGemmaTemplate.FormatPrompt(chatHistory, functions);
        

        
        var sb = new StringBuilder();
        await foreach (var token in _executor.InferAsync(prompt, ToLLamaSharpInferenceParams(settings), cancellationToken:cancellationToken))
        {
            sb.Append(token);
        }
        var output = sb.ToString();
        
        var content = new ChatMessageContent(Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant, output);
        
        var calls = FunctionGemmaTemplate.ParseFunctionCalls(output);
        //calls[0].
        if (calls.Any())
        {
            content.Items.Clear(); 
            content.Content = FunctionGemmaTemplate.RemoveFunctionCalls(output);
            
            foreach (var call in calls)
            {
                content.Items.Add(call);
            }
        }
        else 
        {
            if (content.Content != null)
            {
                content.Content = content.Content.Replace("<end_of_turn>", "").Trim();
            }
        }

        return [content];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        SKChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
        var msg = result.FirstOrDefault();
        if (msg != null)
        {
            yield return new StreamingChatMessageContent(Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant, msg.Content);
        }
    }
    
    private InferenceParams GetInferenceParams(PromptExecutionSettings? settings)
    {
        var inferenceParams = new InferenceParams() 
        { 
           
            AntiPrompts = new [] { "<end_of_turn>", "<start_of_turn>" },
            MaxTokens = 4096
        };

       
        return inferenceParams;
    }

    internal static InferenceParams ToLLamaSharpInferenceParams(LLamaSharpPromptExecutionSettings requestSettings)
    {
        if (requestSettings is null)
        {
            throw new ArgumentNullException(nameof(requestSettings));
        }

        var antiPrompts = new List<string>(requestSettings.StopSequences)
        {
            $"{Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User}:",
            $"{Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant}:",
            $"{Microsoft.SemanticKernel.ChatCompletion.AuthorRole.System}:",
            "<start_function_response>", "<end_of_turn>", "<start_of_turn>"
        };
        return new InferenceParams
        {
            AntiPrompts = antiPrompts,
            MaxTokens = requestSettings.MaxTokens ?? 2048,

            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Temperature = (float)requestSettings.Temperature,
                TopP = (float)requestSettings.TopP,
                RepeatPenalty = 1.2f,
                TopK = 40,  
            }
        };
    }
}