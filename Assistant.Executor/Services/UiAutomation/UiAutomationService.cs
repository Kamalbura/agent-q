using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Assistant.Core.Services;

namespace Assistant.Executor.Services.UiAutomation;

public sealed class UiAutomationService : IUiAutomationService
{
    public Task<IReadOnlyList<UiElementModel>> GetTopLevelWindowsAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<UiElementModel>();

        var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
        var root = AutomationElement.RootElement;
        if (root is null)
        {
            return Task.FromResult((IReadOnlyList<UiElementModel>)list);
        }

        var windows = root.FindAll(TreeScope.Children, condition);
        foreach (AutomationElement win in windows)
        {
            try
            {
                var rect = win.Current.BoundingRectangle;
                var id = win.Current.AutomationId ?? string.Empty;
                var name = win.Current.Name ?? string.Empty;
                var type = win.Current.ControlType?.ProgrammaticName ?? "Window";
                var focusable = win.Current.IsKeyboardFocusable;

                list.Add(new UiElementModel(id, name, type, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, focusable));
            }
            catch
            {
                // swallow and continue
            }
        }

        return Task.FromResult((IReadOnlyList<UiElementModel>)list);
    }

    public Task<UiElementModel?> FindElementByAutomationIdAsync(string automationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(automationId))
        {
            return Task.FromResult<UiElementModel?>(null);
        }

        var root = AutomationElement.RootElement;
        if (root is null)
        {
            return Task.FromResult<UiElementModel?>(null);
        }

        var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        var element = root.FindFirst(TreeScope.Descendants, condition);
        if (element is null)
        {
            return Task.FromResult<UiElementModel?>(null);
        }

        try
        {
            var rect = element.Current.BoundingRectangle;
            var focusable = element.Current.IsKeyboardFocusable;
            return Task.FromResult<UiElementModel?>(
                new UiElementModel(
                    element.Current.AutomationId ?? string.Empty,
                    element.Current.Name ?? string.Empty,
                    element.Current.ControlType?.ProgrammaticName ?? string.Empty,
                    (int)rect.X,
                    (int)rect.Y,
                    (int)rect.Width,
                    (int)rect.Height,
                    focusable));
        }
        catch
        {
            return Task.FromResult<UiElementModel?>(null);
        }
    }

    public Task<string?> GetElementTextAsync(UiElementModel element, CancellationToken cancellationToken = default)
    {
        if (element is null)
        {
            return Task.FromResult<string?>(null);
        }

        var root = AutomationElement.RootElement;
        if (root is null)
        {
            return Task.FromResult<string?>(null);
        }

        var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, element.AutomationId);
        var ae = root.FindFirst(TreeScope.Descendants, condition);
        if (ae is null)
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            // Try ValuePattern
            if (ae.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
            {
                var vp = (ValuePattern)pattern;
                return Task.FromResult<string?>(vp.Current.Value);
            }

            // Try TextPattern
            if (ae.TryGetCurrentPattern(TextPattern.Pattern, out var tpattern))
            {
                var tp = (TextPattern)tpattern;
                return Task.FromResult<string?>(tp.DocumentRange.GetText(-1)?.Trim());
            }

            // Fallback to Name
            return Task.FromResult<string?>(ae.Current.Name);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task<UiElementModel?> GetFocusedElementAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var focused = AutomationElement.FocusedElement;
            if (focused is null)
            {
                return Task.FromResult<UiElementModel?>(null);
            }

            var rect = focused.Current.BoundingRectangle;
            var id = focused.Current.AutomationId ?? string.Empty;
            var name = focused.Current.Name ?? string.Empty;
            var type = focused.Current.ControlType?.ProgrammaticName ?? string.Empty;
            var focusable = focused.Current.IsKeyboardFocusable;
            return Task.FromResult<UiElementModel?>(
                new UiElementModel(id, name, type, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, focusable));
        }
        catch
        {
            return Task.FromResult<UiElementModel?>(null);
        }
    }
}
