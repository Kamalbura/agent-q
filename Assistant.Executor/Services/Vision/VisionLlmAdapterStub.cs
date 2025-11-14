using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Serialization;
using Assistant.Core.Services.Vision;
using Serilog;

namespace Assistant.Executor.Services.Vision;

/// <summary>
/// Phase-1 stub that echoes the user intent and current screen context to produce deterministic JSON.
/// </summary>
public sealed class VisionLlmAdapterStub : IVisionLlmAdapter
{
    private readonly ILogger _logger;

    public VisionLlmAdapterStub(ILogger logger)
    {
        _logger = logger;
    }

    public Task<string> GetActionPlanAsync(string intent, ScreenContextDto context, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            intent,
            screen = new
            {
                ocr = context.OcrText,
                uia = context.UiTree.Select(e => new { e.Id, e.Name, e.ControlType }).ToArray(),
                focused = context.FocusedWindowTitle,
                screenshot_id = context.ScreenshotHash,
                timestamp = context.TimestampUtc
            }
        };

        var payloadJson = JsonSerializer.Serialize(payload, JsonSettings.Options);
        _logger.Information("Vision adapter payload: {Payload}", payloadJson);

        var plan = BuildPlan(intent, context);
        var planJson = JsonSerializer.Serialize(plan, JsonSettings.Options);
        return Task.FromResult(planJson);
    }

    private static object[] BuildPlan(string intent, ScreenContextDto context)
    {
        var uiElements = context.UiTree;
        var firstElement = uiElements.FirstOrDefault();
        var ambiguousName = uiElements.Skip(1).FirstOrDefault()?.Name ?? firstElement?.Name ?? "Control";
        // Simple intent mapping: if user asked to open an app, return an open_app action when allowed.
        var normalized = (intent ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.StartsWith("open ") || normalized.Contains("open "))
        {
            // Try to extract an app name after 'open' and map common app names to executables
            var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string appName = parts.Length >= 2 ? string.Join(' ', parts[1..]) : string.Empty;
            string? exe = null;

            // Common app name -> executable mapping
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["notepad"] = "notepad.exe",
                ["calculator"] = "calc.exe",
                ["calc"] = "calc.exe",
                ["paint"] = "mspaint.exe",
                ["mspaint"] = "mspaint.exe",
                ["edge"] = "msedge.exe",
                ["chrome"] = "chrome.exe",
                ["explorer"] = "explorer.exe",
                ["file explorer"] = "explorer.exe",
                ["files"] = "explorer.exe",
                ["code"] = "code.exe",
                ["visual studio code"] = "code.exe",
                ["vscode"] = "code.exe",
                ["word"] = "winword.exe",
                ["excel"] = "excel.exe",
                ["powerpoint"] = "powerpnt.exe",
                ["terminal"] = "wt.exe"
            };

            foreach (var kv in mapping)
            {
                if (appName.Contains(kv.Key, StringComparison.OrdinalIgnoreCase) || normalized.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    exe = kv.Value;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(exe))
            {
                var plan = new List<object>
                {
                    new
                    {
                        type = ActionTypes.OpenApp,
                        args = new
                        {
                            target = exe,
                            metadata = new Dictionary<string, string>
                            {
                                ["source"] = "vision_stub"
                            }
                        }
                    }
                };

                return plan.ToArray();
            }
        }

        // Fallback sample plan (keeps previous sample behavior)
        var clickBounds = new
        {
            x = context.ScreenBounds.X + context.ScreenBounds.Width + 50,
            y = context.ScreenBounds.Y + 10,
            width = 25,
            height = 25
        };

        var fallback = new List<object>
        {
            new
            {
                type = ActionTypes.TypeText,
                args = new
                {
                    text = $"Intent: {intent}",
                    uiElementId = firstElement?.Id,
                    metadata = new Dictionary<string, string>
                    {
                        ["source"] = "vision_stub"
                    }
                }
            },
            new
            {
                type = ActionTypes.ClickByBounds,
                args = new
                {
                    bounds = clickBounds,
                    metadata = new Dictionary<string, string>
                    {
                        ["reason"] = "sample_out_of_bounds"
                    }
                }
            },
            new
            {
                type = ActionTypes.TypeText,
                args = new
                {
                    text = "Ambiguous element selection",
                    metadata = new Dictionary<string, string>
                    {
                        ["name"] = ambiguousName
                    }
                }
            }
        };

        return fallback.ToArray();
    }
}
