using System.Collections.Generic;
using Assistant.Core.Actions;

namespace Assistant.Core.Services.Vision;

public sealed record SafetyEvaluation(
    IReadOnlyList<AssistantAction> SanitizedActions,
    bool RequiresConfirmation,
    string Reason);
