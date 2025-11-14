using System.Text.Json;
using Assistant.Core.Actions;
using Assistant.Core.Serialization;

namespace Assistant.Core.Samples;

public static class ActionPlanSamples
{
    private static readonly IReadOnlyList<AssistantAction> DefaultPlan = new List<AssistantAction>
    {
        new()
        {
            Type = ActionTypes.OpenApp,
            Args = new ActionArgs { Target = "notepad.exe" }
        },
        new()
        {
            Type = ActionTypes.TypeText,
            Args = new ActionArgs { Text = "Hello from Astra." }
        },
        new()
        {
            Type = ActionTypes.Wait,
            Args = new ActionArgs { DelayMs = 500 }
        }
    };

    public static string DefaultPlanJson => JsonSerializer.Serialize(DefaultPlan, JsonSettings.Options);

    public static IReadOnlyList<AssistantAction> DefaultActions => DefaultPlan;
}
