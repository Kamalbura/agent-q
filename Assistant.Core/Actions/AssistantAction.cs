using System.Collections.Generic;
using System.Text.Json.Serialization;
using Assistant.Core.Context;

namespace Assistant.Core.Actions;

public sealed class AssistantAction
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public ActionArgs Args { get; set; } = new();
}

public sealed class ActionArgs
{
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("delayMs")]
    public int? DelayMs { get; set; }

    [JsonPropertyName("uiElementId")]
    public string? UiElementId { get; set; }

    [JsonPropertyName("bounds")]
    public Bounds? Bounds { get; set; }

    [JsonPropertyName("requireConfirmation")]
    public bool RequireConfirmation { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}
