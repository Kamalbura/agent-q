using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Core.Services;

public sealed record TextRegion(string Text, int X, int Y, int Width, int Height);

public sealed record OcrResult(string FullText, IReadOnlyList<TextRegion> Regions);

public sealed record UiElementModel(string AutomationId, string Name, string ControlType, int X, int Y, int Width, int Height, bool IsFocusable = true);

public interface IOcrService
{
    Task<OcrResult> ReadScreenAsync(byte[]? screenshotPng, CancellationToken cancellationToken = default);
}

public interface IUiAutomationService
{
    Task<IReadOnlyList<UiElementModel>> GetTopLevelWindowsAsync(CancellationToken cancellationToken = default);

    Task<UiElementModel?> FindElementByAutomationIdAsync(string automationId, CancellationToken cancellationToken = default);

    Task<string?> GetElementTextAsync(UiElementModel element, CancellationToken cancellationToken = default);

    Task<UiElementModel?> GetFocusedElementAsync(CancellationToken cancellationToken = default);
}
