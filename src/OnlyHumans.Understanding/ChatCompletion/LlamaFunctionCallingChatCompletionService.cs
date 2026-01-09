using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SKChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

namespace OnlyHumans.ChatCompletion;

public class LlamaFunctionCallingChatCompletionService : IChatCompletionService
{
    private readonly StatelessExecutor _executor;

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public LlamaFunctionCallingChatCompletionService(StatelessExecutor executor)
    {
        _executor = executor;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        SKChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        List<KernelFunction>? functions = null;
        if (executionSettings?.FunctionChoiceBehavior != null && kernel != null)
        {
             functions = kernel.Plugins.SelectMany(p => p).ToList();
        }

        string prompt = FunctionGemmaFormatter.FormatPrompt(chatHistory, functions);
        
        var inferenceParams = GetInferenceParams(executionSettings);
        
        var sb = new StringBuilder();
        await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
        {
            sb.Append(token);
        }
        var output = sb.ToString();
        
        var content = new ChatMessageContent(Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Assistant, output);
        
        var calls = FunctionGemmaFormatter.ParseFunctionCalls(output);
        if (calls.Any())
        {
            content.Items.Clear(); 
            content.Content = FunctionGemmaFormatter.RemoveFunctionCalls(output);
            
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

        return new[] { content };
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
}