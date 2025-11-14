using Assistant.Core.Actions;
using Assistant.Core.Context;

namespace Assistant.Executor.Abstractions;

public interface IActionExecutor
{
    Task ExecutePlanAsync(IReadOnlyList<AssistantAction> actions, ScreenContextDto context, CancellationToken cancellationToken = default);

    bool TryParseActions(string planJson, out IReadOnlyList<AssistantAction> actions, out IReadOnlyList<string> errors);
}
