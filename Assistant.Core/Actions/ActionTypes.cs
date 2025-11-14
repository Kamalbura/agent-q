namespace Assistant.Core.Actions;

public static class ActionTypes
{
    public const string OpenApp = "open_app";
    public const string TypeText = "type_text";
    public const string Wait = "wait";
    public const string ClickByBounds = "click_by_bounds";

    private static readonly HashSet<string> KnownTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        OpenApp,
        TypeText,
        Wait,
        ClickByBounds
    };

    public static bool IsKnown(string? type) => !string.IsNullOrWhiteSpace(type) && KnownTypes.Contains(type);

    public static string Normalize(string type) => type.Trim().ToLowerInvariant();
}
