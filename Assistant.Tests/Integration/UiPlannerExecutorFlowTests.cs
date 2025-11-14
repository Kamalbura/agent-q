using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Services.Vision;
using Assistant.Executor;
using Assistant.Executor.Abstractions;
using Assistant.Executor.Planning;
using Assistant.Executor.Services.Safety;
using NUnit.Framework;
using Serilog;

namespace Assistant.Tests.Integration;

[TestFixture]
public sealed class UiPlannerExecutorFlowTests
{
    [Test]
    public async Task Pipeline_sets_confirmation_and_drops_out_of_bounds_actions()
    {
        var logger = new LoggerConfiguration().CreateLogger();
        var context = BuildContext();
        var adapter = new FakeVisionAdapter();
        var generator = new LlmPlanGenerator(adapter, logger);
        var safety = new SimpleSafetyLayer(logger);
        var textEntry = new SpyTextEntry();
        var processLauncher = new SpyProcessLauncher();
        var executor = new ActionExecutor(textEntry, processLauncher, logger);

        var planResult = await generator.GenerateAsync("Open app", context);
        Assert.That(planResult.Success, Is.True);
        Assert.That(planResult.Actions, Has.Count.GreaterThanOrEqualTo(3));

        var safetyResult = safety.Evaluate(planResult.Actions, context);
        Assert.That(safetyResult.SanitizedActions.Count, Is.EqualTo(3));
        Assert.That(safetyResult.RequiresConfirmation, Is.True);
        Assert.That(safetyResult.SanitizedActions.Any(a => a.Type == ActionTypes.ClickByBounds), Is.False, "Out-of-bounds action should be dropped");

        foreach (var action in safetyResult.SanitizedActions)
        {
            action.Args.RequireConfirmation = false;
        }

        await executor.ExecutePlanAsync(safetyResult.SanitizedActions, context);

        Assert.That(processLauncher.LaunchedTargets, Contains.Item("notepad.exe"));
        Assert.That(textEntry.TextEntries.Count, Is.GreaterThanOrEqualTo(1));
    }

    private static ScreenContextDto BuildContext()
    {
        var elements = new List<UiElementDto>
        {
            new("ok-1", "ok-automation-1", "OK", "Button", new Rect(10, 10, 100, 30), Array.Empty<UiElementDto>(), true),
            new("ok-2", "ok-automation-2", "OK", "Button", new Rect(120, 10, 100, 30), Array.Empty<UiElementDto>(), true),
            new("approve-1", "approve-automation-1", "Approve", "Button", new Rect(230, 10, 100, 30), Array.Empty<UiElementDto>(), true),
            new("approve-2", "approve-automation-2", "Approve", "Button", new Rect(350, 10, 100, 30), Array.Empty<UiElementDto>(), true)
        };

        return new ScreenContextDto(
            "path",
            "hash",
            null,
            string.Empty,
            elements,
            "Window",
            new Rect(0, 0, 1920, 1080),
            DateTime.UtcNow,
            null);
    }

    private sealed class FakeVisionAdapter : IVisionLlmAdapter
    {
        public Task<string> GetActionPlanAsync(string intent, ScreenContextDto context, System.Threading.CancellationToken cancellationToken = default)
        {
            var plan = new object[]
            {
                new
                {
                    type = ActionTypes.OpenApp,
                    args = new { target = "notepad.exe" }
                },
                new
                {
                    type = ActionTypes.ClickByBounds,
                    args = new
                    {
                        bounds = new { x = 5000, y = 5, width = 10, height = 10 }
                    }
                },
                new
                {
                    type = ActionTypes.TypeText,
                    args = new
                    {
                        text = "Hello",
                        uiElementId = "ok-1"
                    }
                },
                new
                {
                    type = ActionTypes.TypeText,
                    args = new
                    {
                        text = "Approve please",
                        metadata = new Dictionary<string, string> { ["name"] = "Approve" }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(plan);
            return Task.FromResult(json);
        }
    }

    private sealed class SpyTextEntry : ITextEntrySimulator
    {
        public List<string> TextEntries { get; } = new();

        public void EnterText(string text)
        {
            TextEntries.Add(text);
        }
    }

    private sealed class SpyProcessLauncher : IProcessLauncher
    {
        public List<string> LaunchedTargets { get; } = new();

        public void Launch(string target)
        {
            LaunchedTargets.Add(target);
        }
    }
}
