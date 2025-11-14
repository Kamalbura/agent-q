using System.Threading.Tasks;
using Assistant.Core.Services;
using Assistant.Executor.Services.Ocr;
using Assistant.Executor.Services.UiAutomation;

namespace Assistant.Tests;

public sealed class ScreenUnderstandingTests
{
    [Test]
    public async Task NullOcrService_returns_empty()
    {
        var ocr = new NullOcrService();
        var res = await ocr.ReadScreenAsync(null);

        Assert.That(res.FullText, Is.Empty);
        Assert.That(res.Regions, Is.Empty);
    }

    [Test]
    public async Task UiAutomationService_constructs_and_can_call_methods()
    {
        var ui = new UiAutomationService();
        var windows = await ui.GetTopLevelWindowsAsync();

        Assert.That(windows, Is.Not.Null);
        // We can't assert on count since environments vary; ensure call completed
    }
}
