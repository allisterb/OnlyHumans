using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OnlyHumans.Acp
{
    /// <summary>
    /// Text content. May be plain text or formatted with Markdown.
    /// </summary>
    [JsonInheritance("text", typeof(ContentBlockText))]
    public partial record ContentBlockText : ContentBlock
    {
        [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public object? _meta { get; set; }

        [JsonProperty("annotations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Annotations? Annotations { get; set; }

        [JsonProperty("text", Required = Required.Always)]
        [Required(AllowEmptyStrings = false)]
        public string Text { get; set; } = "";
    }

    /// <summary>
    /// Images for visual context or analysis.
    /// </summary>
    [JsonInheritance("image", typeof(ContentBlockImage))]
    public partial record ContentBlockImage : ContentBlock
    {
        [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public object? _meta { get; set; }

        [JsonProperty("annotations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Annotations? Annotations { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Data { get; set; } = "";

        [JsonProperty("mimeType", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string MimeType { get; set; } = "";

        [JsonProperty("uri", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string? Uri { get; set; } 
    }

    /// <summary>
    /// Audio data for transcription or analysis.
    /// </summary>
    [JsonInheritance("audio", typeof(ContentBlockAudio))]
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.5.2.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial record ContentBlockAudio : ContentBlock
    {
        [JsonProperty("_meta", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public object? _meta { get; set; }

        [JsonProperty("annotations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Annotations? Annotations { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Data { get; set; } = "";

        [JsonProperty("mimeType", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string MimeType { get; set; } = "";
    }

    /// <summary>
    /// References to resources that the agent can access.
    /// </summary>
    [JsonInheritance("resource_link", typeof(ContentBlockResourceLink))]
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.5.2.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial record ContentBlockResourceLink : ContentBlock
    {
        [JsonProperty("_meta", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public object? _meta { get; set; }

        [JsonProperty("annotations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Annotations? Annotations { get; set; }

        [JsonProperty("description", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("mimeType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string? MimeType { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Name { get; set; } = "";

        [JsonProperty("size", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("title", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("uri", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Uri { get; set; } = "";
    }

    /// <summary>
    /// Complete resource contents embedded directly in the message.
    /// </summary>
    [JsonInheritance("resource", typeof(ContentBlockResource))]
    public partial record ContentBlockResource : ContentBlock
    {
        [JsonProperty("_meta", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public object? _meta { get; set; }

        [JsonProperty("annotations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Annotations? Annotations { get; set; }

        [JsonProperty("resource", Required = Required.Always)]
        [Required]
        public EmbeddedResourceResource Resource { get; set; } = new EmbeddedResourceResource();
    }



  
        /// <summary>
        /// A chunk of the user's message being streamed.
        /// </summary>
        [JsonInheritance("user_message_chunk", typeof(SessionUpdateUserMessageChunk))]
        public partial record SessionUpdateUserMessageChunk : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("content", Required = Required.Always)]
            [Required]
            public ContentBlock Content { get; set; } = null!;

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "user_message_chunk";
        }

        /// <summary>
        /// A chunk of the agent's response being streamed.
        /// </summary>
        [JsonInheritance("agent_message_chunk", typeof(SessionUpdateAgentMessageChunk))]
        public partial record SessionUpdateAgentMessageChunk : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("content", Required = Required.Always)]
            [Required]
            public ContentBlock Content { get; set; } = null!;

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "agent_message_chunk";
        }

        /// <summary>
        /// A chunk of the agent's internal reasoning being streamed.
        /// </summary>
        [JsonInheritance("agent_thought_chunk", typeof(SessionUpdateAgentThoughtChunk))]
        public partial record SessionUpdateAgentThoughtChunk : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("content", Required = Required.Always)]
            [Required]
            public ContentBlock Content { get; set; } = null!;

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "agent_thought_chunk";
        }

        /// <summary>
        /// Notification that a new tool call has been initiated.
        /// </summary>
        [JsonInheritance("tool_call", typeof(SessionUpdateToolCall))]
        public partial record SessionUpdateToolCall : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("content", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<ToolCallContent>? Content { get; set; }

            [JsonProperty("kind", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string? Kind { get; set; }

            [JsonProperty("locations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<ToolCallLocation>? Locations { get; set; }

            [JsonProperty("rawInput", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? RawInput { get; set; }

            [JsonProperty("rawOutput", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? RawOutput { get; set; }

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "tool_call";

            [JsonProperty("status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string? Status { get; set; }

            [JsonProperty("title", Required = Required.Always)]
            [Required(AllowEmptyStrings = true)]
            public string Title { get; set; } = "";

            [JsonProperty("toolCallId", Required = Required.Always)]
            [Required(AllowEmptyStrings = true)]
            public string ToolCallId { get; set; } = "";
        }

        /// <summary>
        /// Update on the status or results of a tool call.
        /// </summary>
        [JsonInheritance("tool_call_update", typeof(SessionUpdateToolCallUpdate))]
        public partial record SessionUpdateToolCallUpdate : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("content", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<ToolCallContent>? Content { get; set; }

            [JsonProperty("kind", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string? Kind { get; set; }

            [JsonProperty("locations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<ToolCallLocation>? Locations { get; set; }

            [JsonProperty("rawInput", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? RawInput { get; set; }

            [JsonProperty("rawOutput", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? RawOutput { get; set; }

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "tool_call_update";

            [JsonProperty("status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string? Status { get; set; }

            [JsonProperty("title", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string? Title { get; set; }

            [JsonProperty("toolCallId", Required = Required.Always)]
            [Required(AllowEmptyStrings = true)]
            public string ToolCallId { get; set; } = "";
        }

        /// <summary>
        /// The agent's execution plan for complex tasks.
        /// </summary>
        [JsonInheritance("plan", typeof(SessionUpdatePlan))]
        public partial record SessionUpdatePlan : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("entries", Required = Required.Always)]
            [Required]
            public ICollection<PlanEntry> Entries { get; set; } = new List<PlanEntry>();

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "plan";
        }

        /// <summary>
        /// Available commands are ready or have changed.
        /// </summary>
        [JsonInheritance("available_commands_update", typeof(SessionUpdateAvailableCommandsUpdate))]
        public partial record SessionUpdateAvailableCommandsUpdate : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("availableCommands", Required = Required.Always)]
            [Required]
            public ICollection<AvailableCommand> AvailableCommands { get; set; } = new List<AvailableCommand>();

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "available_commands_update";
        }

        /// <summary>
        /// The current mode of the session has changed.
        /// </summary>
        [JsonInheritance("current_mode_update", typeof(SessionUpdateCurrentModeUpdate))]
        public partial record SessionUpdateCurrentModeUpdate : SessionUpdate
        {
            [JsonProperty("_meta", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public object? _meta { get; set; }

            [JsonProperty("currentModeId", Required = Required.Always)]
            [Required(AllowEmptyStrings = true)]
            public string CurrentModeId { get; set; } = "";

            [JsonProperty("sessionUpdate", Required = Required.Always)]
            [JsonConverter(typeof(StringEnumConverter))]
            public string SessionUpdateType { get; set; } = "current_mode_update";
        }
    
}