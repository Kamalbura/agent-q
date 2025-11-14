using System;
using System.Collections.Generic;
using System.Linq;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Services.Vision;
using Serilog;

namespace Assistant.Executor.Services.Safety;

/// <summary>
/// Safety layer that validates UI references, bounds, ambiguity, and basic allowlists.
/// </summary>
public sealed class SimpleSafetyLayer : ISafetyLayer
{
    private readonly HashSet<string> _allowlist = new(StringComparer.OrdinalIgnoreCase) { "notepad.exe", "calc.exe" };
    private readonly ILogger _logger;

    public SimpleSafetyLayer(ILogger logger)
    {
        _logger = logger;
    }

    public SafetyEvaluation Evaluate(IReadOnlyList<AssistantAction> actions, ScreenContextDto context)
    {
        if (actions.Count == 0)
        {
            return new SafetyEvaluation(Array.Empty<AssistantAction>(), false, "No actions provided.");
        }

        var flattened = Flatten(context.UiTree);
        var nameLookup = BuildNameLookup(flattened.Values);
        var sanitized = new List<AssistantAction>(actions.Count);
        var requiresConfirmation = false;
        string reason = string.Empty;

        foreach (var action in actions)
        {
            var clone = CloneAction(action);

            if (!ValidateCoreRules(clone, out reason))
            {
                LogFailure(context, clone, reason);
                return new SafetyEvaluation(Array.Empty<AssistantAction>(), false, reason);
            }

            if (!ValidateUiReference(clone, flattened, nameLookup, ref requiresConfirmation, ref reason))
            {
                LogFailure(context, clone, reason);
                return new SafetyEvaluation(Array.Empty<AssistantAction>(), false, reason);
            }

            if (!ValidateBounds(clone, context.ScreenBounds, out var boundsReason, out var dropAction))
            {
                if (dropAction)
                {
                    _logger.Warning("Dropping action {Type}: {Reason}", clone.Type, boundsReason);
                    reason = boundsReason;
                    continue;
                }

                LogFailure(context, clone, boundsReason);
                return new SafetyEvaluation(Array.Empty<AssistantAction>(), false, boundsReason);
            }

            sanitized.Add(clone);
        }

        if (sanitized.Count == 0)
        {
            reason = string.IsNullOrEmpty(reason) ? "All actions were dropped." : reason;
            return new SafetyEvaluation(Array.Empty<AssistantAction>(), requiresConfirmation, reason);
        }

        _logger.Information("Safety evaluation complete for {Hash}, requireConfirmation={RequiresConfirmation}", context.ScreenshotHash, requiresConfirmation);
        return new SafetyEvaluation(sanitized, requiresConfirmation, reason);
    }

    private bool ValidateCoreRules(AssistantAction action, out string reason)
    {
        if (action.Type == ActionTypes.OpenApp)
        {
            var target = action.Args.Target ?? string.Empty;
            if (!_allowlist.Contains(target.Trim()))
            {
                reason = $"open_app target '{target}' is not in allowlist.";
                return false;
            }
        }

        if (action.Type == ActionTypes.TypeText)
        {
            var text = action.Args.Text ?? string.Empty;
            if (text.Contains("\n", StringComparison.Ordinal) || text.Contains(';'))
            {
                reason = "type_text contains potentially unsafe characters.";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }

    private static bool ValidateUiReference(
        AssistantAction action,
        IReadOnlyDictionary<string, UiElementDto> flattened,
        IReadOnlyDictionary<string, IReadOnlyList<UiElementDto>> nameLookup,
        ref bool requiresConfirmation,
        ref string reason)
    {
        if (!string.IsNullOrWhiteSpace(action.Args.UiElementId))
        {
            if (!flattened.TryGetValue(action.Args.UiElementId!, out var element))
            {
                reason = $"Unknown UI element '{action.Args.UiElementId}'.";
                return false;
            }

            if (IsAmbiguous(element.Name, nameLookup))
            {
                requiresConfirmation = true;
                action.Args.RequireConfirmation = true;
                reason = $"Multiple elements share the name '{element.Name}'.";
            }

            return true;
        }

        if (action.Args.Metadata is not null && action.Args.Metadata.TryGetValue("name", out var nameRef))
        {
            var key = nameRef.ToLowerInvariant();
            if (!nameLookup.TryGetValue(key, out var matches) || matches.Count == 0)
            {
                reason = $"No UI element named '{nameRef}' was found.";
                return false;
            }

            if (matches.Count > 1)
            {
                requiresConfirmation = true;
                action.Args.RequireConfirmation = true;
                reason = $"Multiple elements named '{nameRef}' detected.";
            }
            else
            {
                action.Args.UiElementId = matches[0].Id;
            }
        }

        reason = string.Empty;
        return true;
    }

    private static bool ValidateBounds(AssistantAction action, Rect screenBounds, out string reason, out bool dropAction)
    {
        if (action.Type != ActionTypes.ClickByBounds)
        {
            reason = string.Empty;
            dropAction = false;
            return true;
        }

        if (action.Args.Bounds is null)
        {
            reason = "click_by_bounds missing bounds.";
            dropAction = false;
            return false;
        }

        if (!screenBounds.Contains(action.Args.Bounds.Value.ToRect()))
        {
            reason = "click_by_bounds bounds outside screen.";
            dropAction = true;
            return false;
        }

        reason = string.Empty;
        dropAction = false;
        return true;
    }

    private static Dictionary<string, UiElementDto> Flatten(IReadOnlyList<UiElementDto> uiTree)
    {
        var dictionary = new Dictionary<string, UiElementDto>(StringComparer.OrdinalIgnoreCase);

        void Recurse(UiElementDto element)
        {
            dictionary[element.Id] = element;
            foreach (var child in element.Children)
            {
                Recurse(child);
            }
        }

        foreach (var element in uiTree)
        {
            Recurse(element);
        }

        return dictionary;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<UiElementDto>> BuildNameLookup(IEnumerable<UiElementDto> elements)
    {
        var lookup = new Dictionary<string, List<UiElementDto>>(StringComparer.OrdinalIgnoreCase);
        foreach (var element in elements)
        {
            if (string.IsNullOrWhiteSpace(element.Name))
            {
                continue;
            }

            if (!lookup.TryGetValue(element.Name, out var list))
            {
                list = new List<UiElementDto>();
                lookup[element.Name] = list;
            }

            list.Add(element);
        }

        return lookup.ToDictionary(kvp => kvp.Key.ToLowerInvariant(), kvp => (IReadOnlyList<UiElementDto>)kvp.Value);
    }

    private static bool IsAmbiguous(string name, IReadOnlyDictionary<string, IReadOnlyList<UiElementDto>> lookup)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var key = name.ToLowerInvariant();
        return lookup.TryGetValue(key, out var matches) && matches.Count > 1;
    }

    private static AssistantAction CloneAction(AssistantAction source)
    {
        return new AssistantAction
        {
            Type = source.Type,
            Args = new ActionArgs
            {
                Target = source.Args.Target,
                Text = source.Args.Text,
                DelayMs = source.Args.DelayMs,
                UiElementId = source.Args.UiElementId,
                Bounds = source.Args.Bounds,
                RequireConfirmation = source.Args.RequireConfirmation,
                Metadata = source.Args.Metadata is null
                    ? null
                    : new Dictionary<string, string>(source.Args.Metadata)
            }
        };
    }

    private void LogFailure(ScreenContextDto context, AssistantAction action, string reason)
    {
        _logger.Warning(
            "Safety rejection for screenshot {Hash} action {Type}: {Reason}",
            context.ScreenshotHash,
            action.Type,
            reason);
    }
}
