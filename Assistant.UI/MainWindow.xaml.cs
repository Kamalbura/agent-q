using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Services.Vision;
using Assistant.Executor.Abstractions;
using Serilog;

namespace Assistant.UI;

public partial class MainWindow : Window
{
    private readonly IScreenContextCollector _contextCollector;
    private readonly ILlmPlanGenerator _planGenerator;
    private readonly ISafetyLayer _safetyLayer;
    private readonly IActionExecutor _actionExecutor;
    private readonly ILogger _logger;
    private HwndSource? _hwndSource;
    private bool _isBusy;
    private string? _previousActionsSummary;

    private const int OperationTimeoutSeconds = 30;

    private const int HotkeyId = 0xA11;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint VkSpace = 0x20;

    public MainWindow(
        IScreenContextCollector contextCollector,
        ILlmPlanGenerator planGenerator,
        ISafetyLayer safetyLayer,
        IActionExecutor actionExecutor,
        ILogger logger)
    {
        _contextCollector = contextCollector;
        _planGenerator = planGenerator;
        _safetyLayer = safetyLayer;
        _actionExecutor = actionExecutor;
        _logger = logger;
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        _hwndSource = HwndSource.FromHwnd(handle);
        _hwndSource?.AddHook(WndProc);
        if (!RegisterHotKey(handle, HotkeyId, ModControl, VkSpace))
        {
            _logger.Warning("CTRL+SPACE hotkey registration failed");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_hwndSource is not null)
        {
            _hwndSource.RemoveHook(WndProc);
            UnregisterHotKey(_hwndSource.Handle, HotkeyId);
        }
    }

    private async void OnListenClicked(object sender, RoutedEventArgs e)
    {
        await RunAssistantAsync();
    }

    private async Task RunAssistantAsync()
    {
        if (_isBusy)
        {
            return;
        }

        var prompt = PromptTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            UpdateStatus("Enter a request");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(OperationTimeoutSeconds));

        try
        {
            _isBusy = true;
            ListenButton.IsEnabled = false;
            UpdateStatus("Capturing context...");

            var context = await _contextCollector.CaptureContextAsync(cts.Token).ConfigureAwait(true);
            if (!string.IsNullOrEmpty(_previousActionsSummary))
            {
                context = context with { PreviousActionsSummary = _previousActionsSummary };
            }

            UpdateStatus("Planning...");
            var planResult = await _planGenerator.GenerateAsync(prompt, context, cts.Token).ConfigureAwait(true);
            if (!planResult.Success)
            {
                UpdateStatus("Plan failed");
                _logger.Warning("Plan generation failed: {Errors}", string.Join(";", planResult.Errors));
                return;
            }

            UpdateStatus("Safety review...");
            var safety = _safetyLayer.Evaluate(planResult.Actions, context);
            if (safety.SanitizedActions.Count == 0)
            {
                UpdateStatus("Blocked");
                _logger.Warning("Safety blocked execution: {Reason}", safety.Reason);
                return;
            }

            var actionsToExecute = new List<AssistantAction>(safety.SanitizedActions.Count);
            foreach (var action in safety.SanitizedActions)
            {
                actionsToExecute.Add(CloneForExecution(action));
            }

            if (safety.RequiresConfirmation)
            {
                var confirmed = ShowConfirmationDialog(safety, context);
                if (!confirmed)
                {
                    UpdateStatus("Canceled");
                    return;
                }

                foreach (var action in actionsToExecute)
                {
                    action.Args.RequireConfirmation = false;
                }
            }

            UpdateStatus("Executing...");
            await _actionExecutor.ExecutePlanAsync(actionsToExecute, context, cts.Token).ConfigureAwait(true);
            _previousActionsSummary = string.Join(", ", actionsToExecute.Select(a => a.Type));
            UpdateStatus("Done");
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Timed out");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Assistant run failed");
            UpdateStatus("Failed");
        }
        finally
        {
            ListenButton.IsEnabled = true;
            _isBusy = false;
        }
    }

    private void UpdateStatus(string text) => StatusTextBlock.Text = text;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            ToggleOverlay();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void ToggleOverlay()
    {
        if (!IsVisible)
        {
            Show();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        if (IsVisible && IsActive)
        {
            Hide();
            return;
        }

        if (!IsVisible)
        {
            Show();
        }

        Activate();
        PromptTextBox.Focus();
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private static AssistantAction CloneForExecution(AssistantAction source)
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
                Metadata = source.Args.Metadata is null ? null : new Dictionary<string, string>(source.Args.Metadata)
            }
        };
    }

    private bool ShowConfirmationDialog(SafetyEvaluation evaluation, ScreenContextDto context)
    {
        var ambiguousItems = evaluation.SanitizedActions
            .Where(a => a.Args.RequireConfirmation)
            .Select(a => a.Args.Metadata?.GetValueOrDefault("name") ?? a.Args.UiElementId ?? a.Type)
            .ToList();

        var dialog = new ConfirmationDialog(ambiguousItems, context.FocusedWindowTitle)
        {
            Owner = this
        };

        return dialog.ShowDialog() == true;
    }
}