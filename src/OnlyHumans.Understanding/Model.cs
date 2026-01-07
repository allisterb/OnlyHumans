namespace OnlyHumans;

using LLama;
using LLama.Native;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using LLamaSharp.SemanticKernel.TextEmbedding;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using OnlyHumans.ChatCompletion;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

public enum ModelRuntime
{
    Ollama,
    LlamaCpp,
    OpenAI
}

public record Model
{
    public ModelRuntime Runtime { get; }
    public string Name { get; }
    public string PathorUrl { get; }
    public object? ModelParamsorConfig { get; } 
    public Uri? DownloadUri { get; }
    public Model(ModelRuntime runtime, string name, string pathorUrl, object? modelParamsorConfig = null, Uri? downloadUri = null)
    {
        this.Runtime = runtime; 
        this.Name = name;   
        this.PathorUrl = pathorUrl; 
        this.ModelParamsorConfig = modelParamsorConfig; 
        this.DownloadUri = downloadUri;
    }
}

public class ModelConversation : Runtime
{
    #region Constructors
    public ModelConversation(Model model, Model? embeddingModel = null, string[]? systemPrompts = null, (IPlugin, string)[]? plugins = null)
    {        
        this.model = model;    
        this.embeddingModel = embeddingModel ?? model;
        chatHistoryMaxLength = Int32.Parse(config?["Model: ChatHistoryMaxLength"] ?? "5");
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(builder =>
            builder
                .SetMinimumLevel(LogLevel.Trace)
                .AddProvider(loggerProvider)
            );
        if (model.Runtime == ModelRuntime.Ollama)
        {            
#pragma warning disable SKEXP0001, SKEXP0070 
            var _client = new OllamaApiClient(new Uri(this.model.PathorUrl), model.Name);            
            if (!_client.IsRunningAsync().Result)
            {
                throw new InvalidOperationException($"Ollama API at {this.model.PathorUrl} is not running. Please start the Ollama server.");
            }
            client = _client;
            chat = _client.AsChatCompletionService(kernel.Services)
                .UsingChatHistoryReducer(new ChatHistoryTruncationReducer(chatHistoryMaxLength));
            builder.AddOllamaEmbeddingGenerator(new OllamaApiClient(new Uri(this.embeddingModel.PathorUrl), this.embeddingModel.Name)); 
#pragma warning restore SKEXP0070, SKEXP0001 
            promptExecutionSettings = new OllamaPromptExecutionSettings()
            {
                ModelId = model.Name,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true),
                Temperature = 0.1f,
                TopK = 64,
                TopP = 0.95f,
                ExtensionData = new Dictionary<string, object>()
            };

            Info("Using Ollama API at {0} with model {1}", this.model.PathorUrl, model);
        }
        else if (model.Runtime == ModelRuntime.LlamaCpp)
        {
            NativeLibraryConfig.LLama
                .WithLogCallback(logger)
                .WithCuda(false)
                .WithAutoFallback(true);            
            var parameters = model.ModelParamsorConfig is not null ? (LLama.Common.ModelParams)model.ModelParamsorConfig : new LLama.Common.ModelParams(model.PathorUrl)
            {
                
            };
            LLamaWeights lm = LLamaWeights.LoadFromFile(parameters);
            var ex = new StatelessExecutor(lm, parameters, logger);
            promptExecutionSettings = new LLamaSharpPromptExecutionSettings()
            {
                ModelId = model.Name,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true),
                ExtensionData = new Dictionary<string, object>(),
            };
            chat = new LLamaSharpChatCompletion(ex).UsingChatHistoryReducer(new ChatHistoryTruncationReducer(chatHistoryMaxLength));
#pragma warning disable SKEXP0001,SKEXP0010 
            client = chat.AsChatClient();
            var embeddingParameters = this.embeddingModel.ModelParamsorConfig is not null ? (LLama.Common.ModelParams) this.embeddingModel.ModelParamsorConfig : new LLama.Common.ModelParams(embeddingModel.PathorUrl);
            embeddingParameters.ModelPath = this.embeddingModel.PathorUrl;   
            var elm = LLamaWeights.LoadFromFile(embeddingParameters);
            var embedding = new LLamaEmbedder(elm, embeddingParameters);
            builder.Services.AddEmbeddingGenerator(embedding);
            Info("Using llama.cpp embedded library with model {0} at {1}.", this.model.Name, this,model.PathorUrl);
        }
        else
        {
            var apiKey = config?["Model:ApiKey"] ?? throw new Exception(); ;
            var apiKeyCred = new System.ClientModel.ApiKeyCredential(apiKey);
            var endpoint = new Uri(this.model.PathorUrl);
            var oaiOptions = new OpenAI.OpenAIClientOptions() { Endpoint = endpoint };
            chat = new OpenAIChatCompletionService(model.Name, new Uri(this.model.PathorUrl), apiKey:apiKey, loggerFactory: loggerFactory)
                .UsingChatHistoryReducer(new ChatHistoryTruncationReducer(chatHistoryMaxLength));
            client = chat.AsChatClient();
            
            builder.AddOpenAIEmbeddingGenerator(this.embeddingModel.Name, new OpenAI.OpenAIClient(apiKeyCred, oaiOptions));
#pragma warning restore SKEXP0001, SKEXP0010
            promptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                ModelId = model.Name,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true),
                TopP = 0.95f,
                Temperature = 0.1,
                MaxTokens = 2048,
                ExtensionData = new Dictionary<string, object>()
            };
            Info("Using OpenAI compatible API at {0} with model {1}", this.model.PathorUrl, model);
        }
        
        builder.Services            
            .AddChatClient(client)
            .UseFunctionInvocation(loggerFactory, configure =>
            {
                configure.TerminateOnUnknownCalls = true;
            });
        kernel = builder.Build();
        
        vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions() { EmbeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>() });
        
        
        if (systemPrompts is not null)
        {
            foreach (var systemPrompt in systemPrompts)
            {
                messages.AddSystemMessage(systemPrompt);
            }
        }  

        if (plugins is not null)
        {
            foreach (var (plugin, pluginName) in plugins)
            {
                kernel.Plugins.AddFromObject(plugin, pluginName);
            }
        }
    }
    #endregion

    #region Methods and Properties

    public ModelConversation AddPlugin<T>(string pluginName)
    {
        kernel.Plugins.AddFromType<T>(pluginName);
        return this;
    }

    public ModelConversation AddPlugin<T>(T obj, string pluginName)
    {
#pragma warning disable SKEXP0120 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        kernel.Plugins.AddFromObject<T>(obj, jsonSerializerOptions: new System.Text.Json.JsonSerializerOptions(), pluginName: pluginName);
#pragma warning restore SKEXP0120 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        return this;
    }
    public async IAsyncEnumerable<StreamingChatMessageContent> Prompt(string prompt, params object[] args)
    {
        var messageItems = new ChatMessageContentItemCollection()
        {
            new Microsoft.SemanticKernel.TextContent(string.Format(prompt, args))
        };
        messages.AddUserMessage(messageItems);
        StringBuilder sb = new StringBuilder();
        await foreach (var m in chat.GetStreamingChatMessageContentsAsync(messages, promptExecutionSettings, kernel))
        {
            if (m.Content is not null && !string.IsNullOrEmpty(m.Content))
            {
                sb.Append(m.Content);
                yield return m;
            }
        }
        messages.AddAssistantMessage(sb.ToString());
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> Prompt(string prompt, byte[]? imageData, string imageMimeType = "image/png")
    {
        messages.AddUserMessage([
            new Microsoft.SemanticKernel.TextContent(prompt),
            new ImageContent(imageData, imageMimeType),

        ]);
        StringBuilder sb = new StringBuilder();
        await foreach (var m in chat.GetStreamingChatMessageContentsAsync(messages, promptExecutionSettings, kernel))
        {
            if (m.Content is not null && !string.IsNullOrEmpty(m.Content))
            {
                sb.Append(m.Content);
                yield return m;
            }
        }
        messages.AddAssistantMessage(sb.ToString());
    }
    #endregion
   
    #region Fields

    public readonly Model model;

    public readonly Model embeddingModel;

    public readonly Kernel kernel = new Kernel();

    public readonly IChatClient client;

    public readonly IChatCompletionService chat;
   
    public readonly ChatHistory messages = new ChatHistory();

    public readonly PromptExecutionSettings promptExecutionSettings;

    public readonly VectorStore vectorStore;

    public int chatHistoryMaxLength;

    public static IConfigurationRoot? config = null;
    #endregion
}
