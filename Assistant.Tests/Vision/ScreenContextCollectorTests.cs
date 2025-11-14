using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Context;
using Assistant.Core.Services;
using Assistant.Core.Services.Vision;
using Assistant.Executor.Services.Context;
using NUnit.Framework;
using Serilog;

namespace Assistant.Tests.Vision;

[TestFixture]
public sealed class ScreenContextCollectorTests
{
    [Test]
    public async Task CaptureContext_returns_expected_dto()
    {
        var collector = new ScreenContextCollector(
            new FakeScreenCaptureService(),
            new FakeOcrService(),
            new FakeUiAutomationService(),
            new LoggerConfiguration().CreateLogger());

        var dto = await collector.CaptureContextAsync();

        Assert.That(dto.OcrText, Is.EqualTo("hello"));
        Assert.That(dto.UiTree, Has.Count.EqualTo(1));
        Assert.That(dto.ScreenshotPath, Is.Not.Empty);
        Assert.That(dto.ScreenshotBytes, Is.Not.Null);
    }

    private sealed class FakeScreenCaptureService : IScreenCaptureService
    {
        public Bitmap CapturePrimaryScreen()
        {
            var bmp = new Bitmap(10, 10);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.AliceBlue);
            return bmp;
        }
    }

    private sealed class FakeOcrService : IOcrService
    {
        public Task<OcrResult> ReadScreenAsync(byte[]? screenshotPng, CancellationToken cancellationToken = default)
        {
            var result = new OcrResult("hello", new List<TextRegion>());
            return Task.FromResult(result);
        }
    }

    private sealed class FakeUiAutomationService : IUiAutomationService
    {
        public Task<IReadOnlyList<UiElementModel>> GetTopLevelWindowsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<UiElementModel> list = new[]
            {
                new UiElementModel("auto", "Window", "Window", 0, 0, 400, 300)
            };
            return Task.FromResult(list);
        }

        public Task<UiElementModel?> FindElementByAutomationIdAsync(string automationId, CancellationToken cancellationToken = default) =>
            Task.FromResult<UiElementModel?>(null);

        public Task<string?> GetElementTextAsync(UiElementModel element, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);

        public Task<UiElementModel?> GetFocusedElementAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<UiElementModel?>(new UiElementModel("focused", "Window", "Window", 0, 0, 400, 300));
    }
}
