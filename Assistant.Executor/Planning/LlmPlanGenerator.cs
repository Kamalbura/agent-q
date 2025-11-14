using System;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Actions;
using Assistant.Core.Context;
using Assistant.Core.Services.Vision;
using Assistant.Core.Validation;
using Serilog;

namespace Assistant.Executor.Planning;

public sealed class LlmPlanGenerator : ILlmPlanGenerator
{
    private readonly IVisionLlmAdapter _visionLlmAdapter;
    private readonly ILogger _logger;

    public LlmPlanGenerator(IVisionLlmAdapter visionLlmAdapter, ILogger logger)
    {
        _visionLlmAdapter = visionLlmAdapter;
        _logger = logger;
    }

    public async Task<PlanGenerationResult> GenerateAsync(string intent, ScreenContextDto context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            var errors = new[] { "Intent cannot be empty." };
            return new PlanGenerationResult(false, string.Empty, Array.Empty<AssistantAction>(), errors);
        }

        var planJson = await _visionLlmAdapter.GetActionPlanAsync(intent, context, cancellationToken).ConfigureAwait(false);
        if (!ActionValidator.TryValidate(planJson, out var actions, out var errorsFromValidator))
        {
            _logger.Warning("Plan validation failed: {Errors}", string.Join(";", errorsFromValidator));
            return new PlanGenerationResult(false, planJson, Array.Empty<AssistantAction>(), errorsFromValidator);
        }

        _logger.Information("Generated plan with {Count} actions for screenshot {Hash}", actions.Count, context.ScreenshotHash);
        return new PlanGenerationResult(true, planJson, actions, Array.Empty<string>());
    }
}
