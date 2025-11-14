using Assistant.Core.Actions;
using Assistant.Core.Context;

namespace Assistant.Core.Services.Vision
{
    /// <summary>
    /// Safety layer to inspect parsed actions and veto or annotate them before execution.
    /// </summary>
    public interface ISafetyLayer
    {
        SafetyEvaluation Evaluate(IReadOnlyList<AssistantAction> actions, ScreenContextDto context);
    }
}
