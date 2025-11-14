using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Actions;
using Assistant.Core.Context;

namespace Assistant.Core.Services.Vision;

public interface ILlmPlanGenerator
{
    Task<PlanGenerationResult> GenerateAsync(string intent, ScreenContextDto context, CancellationToken cancellationToken = default);
}

public sealed record PlanGenerationResult(
    bool Success,
    string PlanJson,
    IReadOnlyList<AssistantAction> Actions,
    IReadOnlyList<string> Errors);
