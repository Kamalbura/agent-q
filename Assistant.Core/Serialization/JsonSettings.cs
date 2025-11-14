using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Core.Serialization;

public static class JsonSettings
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };
}
