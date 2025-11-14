using System;
using System.Collections.Generic;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Services.Vision;
using Assistant.Executor.Services.Safety;
using NUnit.Framework;
using Serilog;

namespace Assistant.Tests.Safety;

[TestFixture]
public sealed class SimpleSafetyLayerTests
{
    private readonly ILogger _logger = new LoggerConfiguration().CreateLogger();

    [Test]
    public void UnknownUiElement_blocks_plan()
    {
        var layer = new SimpleSafetyLayer(_logger);
        var context = CreateContext(new List<UiElementDto>());
        var actions = new[]
        {
            new AssistantAction
            {
                Type = ActionTypes.TypeText,
                Args = new ActionArgs
                {
                    Text = "Hello",
                    UiElementId = "missing"
                }
            }
        };

        var evaluation = layer.Evaluate(actions, context);
        Assert.That(evaluation.SanitizedActions, Is.Empty);
        Assert.That(evaluation.Reason, Does.Contain("Unknown UI element"));
    }

    [Test]
    public void OutOfBoundsClick_is_blocked()
    {
        var layer = new SimpleSafetyLayer(_logger);
        var context = CreateContext(Array.Empty<UiElementDto>());
        var actions = new[]
        {
            new AssistantAction
            {
                Type = ActionTypes.ClickByBounds,
                Args = new ActionArgs
                {
                    Bounds = new Bounds(4000, 100, 10, 10)
                }
            }
        };

        var evaluation = layer.Evaluate(actions, context);
        Assert.That(evaluation.SanitizedActions, Is.Empty);
        Assert.That(evaluation.Reason, Does.Contain("outside"));
    }

    [Test]
    public void AmbiguousName_requires_confirmation()
    {
        var layer = new SimpleSafetyLayer(_logger);
        var elementA = CreateElement("id-1", "OK", new Rect(0, 0, 100, 30));
        var elementB = CreateElement("id-2", "OK", new Rect(120, 0, 100, 30));
        var context = CreateContext(new[] { elementA, elementB });
        var actions = new[]
        {
            new AssistantAction
            {
                Type = ActionTypes.TypeText,
                Args = new ActionArgs
                {
                    Text = "Confirm",
                    UiElementId = elementA.Id
                }
            }
        };

        var evaluation = layer.Evaluate(actions, context);
        Assert.That(evaluation.SanitizedActions, Has.Count.EqualTo(1));
        Assert.That(evaluation.RequiresConfirmation, Is.True);
        Assert.That(evaluation.SanitizedActions[0].Args.RequireConfirmation, Is.True);
    }

    private static ScreenContextDto CreateContext(IReadOnlyList<UiElementDto> tree) =>
        new(
            "path",
            "hash",
            null,
            string.Empty,
            tree,
            "Window",
            new Rect(0, 0, 1920, 1080),
            DateTime.UtcNow,
            null);

    private static UiElementDto CreateElement(string id, string name, Rect rect) =>
        new(id, id, name, "Button", rect, Array.Empty<UiElementDto>(), true);
}
