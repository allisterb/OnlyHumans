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
}