using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Validation;
using Assistant.Executor.Abstractions;
using Assistant.Executor.Input;
using Serilog;

namespace Assistant.Executor;

public sealed class ActionExecutor : IActionExecutor
{
    private readonly ITextEntrySimulator _textEntry;
    private readonly IProcessLauncher _processLauncher;
    private readonly ILogger _logger;

    private const uint MouseeventfLeftdown = 0x0002;
    private const uint MouseeventfLeftup = 0x0004;

    public ActionExecutor(ITextEntrySimulator? textEntry = null, IProcessLauncher? processLauncher = null, ILogger? logger = null)
    {
        _textEntry = textEntry ?? new InputSimulatorTextEntry();
        _processLauncher = processLauncher ?? new Process.ProcessLauncher();
        _logger = logger ?? Log.Logger;
    }

    public bool TryParseActions(string planJson, out IReadOnlyList<AssistantAction> actions, out IReadOnlyList<string> errors)
    {
        var isValid = ActionValidator.TryValidate(planJson, out actions, out errors);
        return isValid;
    }

    public async Task ExecutePlanAsync(IReadOnlyList<AssistantAction> actions, ScreenContextDto context, CancellationToken cancellationToken = default)
    {
        if (actions is null || actions.Count == 0)
        {
            _logger.Information("No actions to execute for screenshot {Hash}", context.ScreenshotHash);
            return;
        }

        var lookup = Flatten(context.UiTree);

        foreach (var action in actions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (action.Args.RequireConfirmation)
            {
                throw new InvalidOperationException($"Action {action.Type} still requires confirmation.");
            }

            ValidateAgainstContext(action, context, lookup);

            var sw = Stopwatch.StartNew();
            await ExecuteActionAsync(action, cancellationToken).ConfigureAwait(false);
            sw.Stop();

            _logger.Information(
                "Executed {Type} in {Elapsed}ms (screenshot={Hash}, focus={Focus}, element={Element})",
                action.Type,
                sw.ElapsedMilliseconds,
                context.ScreenshotHash,
                context.FocusedWindowTitle,
                action.Args.UiElementId ?? action.Args.Metadata?.GetValueOrDefault("name") ?? "n/a");
        }
    }

    private static Dictionary<string, UiElementDto> Flatten(IReadOnlyList<UiElementDto> tree)
    {
        var map = new Dictionary<string, UiElementDto>(StringComparer.OrdinalIgnoreCase);
        void Recurse(UiElementDto node)
        {
            map[node.Id] = node;
            foreach (var child in node.Children)
            {
                Recurse(child);
            }
        }

        foreach (var node in tree)
        {
            Recurse(node);
        }

        return map;
    }

    private void ValidateAgainstContext(AssistantAction action, ScreenContextDto context, IReadOnlyDictionary<string, UiElementDto> lookup)
    {
        if (!string.IsNullOrWhiteSpace(action.Args.UiElementId) && !lookup.ContainsKey(action.Args.UiElementId!))
        {
            throw new InvalidOperationException($"Action references unknown UI element '{action.Args.UiElementId}'.");
        }

        if (action.Type == ActionTypes.ClickByBounds)
        {
            if (action.Args.Bounds is null)
            {
                throw new InvalidOperationException("click_by_bounds missing bounds at execution time.");
            }

            if (!context.ScreenBounds.Contains(action.Args.Bounds.Value.ToRect()))
            {
                throw new InvalidOperationException("click_by_bounds bounds fall outside captured screen.");
            }
        }
    }

    private Task ExecuteActionAsync(AssistantAction action, CancellationToken cancellationToken)
    {
        switch (action.Type)
        {
            case ActionTypes.OpenApp:
                _processLauncher.Launch(action.Args.Target!);
                break;
            case ActionTypes.TypeText:
                _textEntry.EnterText(action.Args.Text!);
                break;
            case ActionTypes.Wait:
                return Task.Delay(action.Args.DelayMs!.Value, cancellationToken);
            case ActionTypes.ClickByBounds:
                PerformClick(action.Args.Bounds!.Value);
                break;
        }

        return Task.CompletedTask;
    }

    private static void PerformClick(Bounds bounds)
    {
        var centerX = (int)(bounds.X + bounds.Width / 2);
        var centerY = (int)(bounds.Y + bounds.Height / 2);
        SetCursorPos(centerX, centerY);
        mouse_event(MouseeventfLeftdown, (uint)centerX, (uint)centerY, 0, UIntPtr.Zero);
        mouse_event(MouseeventfLeftup, (uint)centerX, (uint)centerY, 0, UIntPtr.Zero);
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
}
