using System;
using Assistant.Core.Context;
using Assistant.Core.Samples;
using Assistant.Executor;
using Assistant.Executor.Abstractions;

namespace Assistant.Tests;

public sealed class ExecutorExecutionTests
{
    [Test]
    public async Task ExecutePlan_calls_launcher_and_text_entry()
    {
        var launcher = new FakeLauncher();
        var typer = new FakeTextEntry();

        var executor = new ActionExecutor(typer, launcher);

        await executor.ExecutePlanAsync(ActionPlanSamples.DefaultActions, CreateContext());

        Assert.That(launcher.Launched, Is.True);
        Assert.That(launcher.Target, Is.EqualTo("notepad.exe"));
        Assert.That(typer.TextEntered, Is.EqualTo("Hello from Astra."));
    }

    private sealed class FakeLauncher : IProcessLauncher
    {
        public bool Launched { get; private set; }
        public string? Target { get; private set; }

        public void Launch(string target)
        {
            Launched = true;
            Target = target;
        }
    }

    private sealed class FakeTextEntry : ITextEntrySimulator
    {
        public string? TextEntered { get; private set; }

        public void EnterText(string text)
        {
            TextEntered = text;
        }
    }

    private static ScreenContextDto CreateContext() =>
        new(
            "test.png",
            "hash",
            Array.Empty<byte>(),
            string.Empty,
            Array.Empty<UiElementDto>(),
            "TestWindow",
            new Rect(0, 0, 1920, 1080),
            DateTime.UtcNow,
            null);
}
