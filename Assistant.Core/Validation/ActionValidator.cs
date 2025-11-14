using System.Text.Json;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Serialization;

namespace Assistant.Core.Validation;

public static class ActionValidator
{
    public static bool TryValidate(
        string json,
        out IReadOnlyList<AssistantAction> actions,
        out IReadOnlyList<string> errors)
    {
        var parsedActions = new List<AssistantAction>();
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors = new[] { "Action payload cannot be empty." };
            actions = Array.Empty<AssistantAction>();
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors = new[] { "Action payload must be an array." };
                actions = Array.Empty<AssistantAction>();
                return false;
            }

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    validationErrors.Add("Each action entry must be an object.");
                    continue;
                }

                if (!element.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
                {
                    validationErrors.Add("Action type is required.");
                    continue;
                }

                var type = typeElement.GetString()?.Trim() ?? string.Empty;
                if (!ActionTypes.IsKnown(type))
                {
                    validationErrors.Add($"Unsupported action type '{type}'.");
                    continue;
                }

                if (!element.TryGetProperty("args", out var argsElement) || argsElement.ValueKind != JsonValueKind.Object)
                {
                    validationErrors.Add($"Action '{type}' requires an args object.");
                    continue;
                }

                var args = argsElement.Deserialize<ActionArgs>(JsonSettings.Options) ?? new ActionArgs();
                var action = new AssistantAction
                {
                    Type = ActionTypes.Normalize(type),
                    Args = args
                };

                ValidateArgs(action, validationErrors);
                parsedActions.Add(action);
            }
        }
        catch (JsonException ex)
        {
            errors = new[] { $"Invalid JSON: {ex.Message}" };
            actions = Array.Empty<AssistantAction>();
            return false;
        }

        errors = validationErrors;
        actions = parsedActions;
        return validationErrors.Count == 0;
    }

    private static void ValidateArgs(AssistantAction action, ICollection<string> errors)
    {
        switch (action.Type)
        {
            case ActionTypes.OpenApp:
                if (string.IsNullOrWhiteSpace(action.Args.Target))
                {
                    errors.Add("open_app requires args.target");
                }
                break;
            case ActionTypes.TypeText:
                if (string.IsNullOrWhiteSpace(action.Args.Text))
                {
                    errors.Add("type_text requires args.text");
                }
                break;
            case ActionTypes.Wait:
                if (action.Args.DelayMs is null or < 0)
                {
                    errors.Add("wait requires args.delayMs >= 0");
                }
                break;
            case ActionTypes.ClickByBounds:
                if (action.Args.Bounds is null)
                {
                    errors.Add("click_by_bounds requires args.bounds");
                    break;
                }

                var rect = action.Args.Bounds.Value.ToRect();
                if (rect.Width <= 0 || rect.Height <= 0)
                {
                    errors.Add("bounds must have positive width/height");
                }
                break;
        }
    }
}
