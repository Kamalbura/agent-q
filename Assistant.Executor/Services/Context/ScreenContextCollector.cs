using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assistant.Core.Context;
using Assistant.Core.Services;
using Assistant.Core.Services.Vision;
using Serilog;

namespace Assistant.Executor.Services.Context;

public sealed class ScreenContextCollector : IScreenContextCollector
{
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IOcrService _ocrService;
    private readonly IUiAutomationService _uiAutomationService;
    private readonly ILogger _logger;

    public ScreenContextCollector(
        IScreenCaptureService screenCaptureService,
        IOcrService ocrService,
        IUiAutomationService uiAutomationService,
        ILogger logger)
    {
        _screenCaptureService = screenCaptureService;
        _ocrService = ocrService;
        _uiAutomationService = uiAutomationService;
        _logger = logger;
    }

    public async Task<ScreenContextDto> CaptureContextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var bitmap = _screenCaptureService.CapturePrimaryScreen();
        var pngBytes = EncodeBitmap(bitmap);
        var screenshotPath = await PersistScreenshotAsync(pngBytes, cancellationToken).ConfigureAwait(false);
        var hash = Convert.ToHexString(SHA256.HashData(pngBytes));

        var ocrTask = _ocrService.ReadScreenAsync(pngBytes, cancellationToken);
        var uiTask = _uiAutomationService.GetTopLevelWindowsAsync(cancellationToken);
        var focusedTask = _uiAutomationService.GetFocusedElementAsync(cancellationToken);

        await Task.WhenAll(ocrTask, uiTask, focusedTask).ConfigureAwait(false);

        var uiTree = BuildUiTree(uiTask.Result);
        var focusedTitle = focusedTask.Result?.Name ?? string.Empty;
        var screenBounds = GetPrimaryScreenBounds();

        var context = new ScreenContextDto(
            screenshotPath,
            hash,
            pngBytes,
            ocrTask.Result.FullText ?? string.Empty,
            uiTree,
            focusedTitle,
            screenBounds,
            DateTime.UtcNow,
            null);

        _logger.Information("Screen context captured {Hash} with {ElementCount} elements", hash, uiTree.Count);
        return context;
    }

    private static IReadOnlyList<UiElementDto> BuildUiTree(IReadOnlyList<UiElementModel> models)
    {
        if (models.Count == 0)
        {
            return Array.Empty<UiElementDto>();
        }

        var list = new List<UiElementDto>(models.Count);
        for (var i = 0; i < models.Count; i++)
        {
            var model = models[i];
            var id = string.IsNullOrWhiteSpace(model.AutomationId)
                ? GenerateStableId(model, i)
                : model.AutomationId!;

            var dto = new UiElementDto(
                id,
                model.AutomationId,
                model.Name,
                model.ControlType,
                new Rect(model.X, model.Y, model.Width, model.Height),
                Array.Empty<UiElementDto>(),
                model.IsFocusable);

            list.Add(dto);
        }

        return list;
    }

    private static string GenerateStableId(UiElementModel model, int index)
    {
        var basis = $"{model.Name}-{model.ControlType}-{model.X}-{model.Y}-{index}";
        return $"auto-{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(basis))).Substring(0, 12).ToLowerInvariant()}";
    }

    private static Rect GetPrimaryScreenBounds()
    {
        var screen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
        if (screen is null)
        {
            return new Rect(0, 0, 1920, 1080);
        }

        var bounds = screen.Bounds;
        return new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private static byte[] EncodeBitmap(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    private static async Task<string> PersistScreenshotAsync(byte[] pngBytes, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(Path.GetTempPath(), "astra-context");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"screen_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.png");
        await File.WriteAllBytesAsync(path, pngBytes, cancellationToken).ConfigureAwait(false);
        return path;
    }
}
