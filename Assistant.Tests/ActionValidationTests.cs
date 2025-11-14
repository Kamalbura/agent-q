using Assistant.Core.Samples;
using Assistant.Core.Validation;

namespace Assistant.Tests;

public sealed class ActionValidationTests
{
    [Test]
    public void Validator_accepts_sample_plan()
    {
        var success = ActionValidator.TryValidate(ActionPlanSamples.DefaultPlanJson, out var actions, out var errors);

        Assert.That(success, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(actions.Count, Is.EqualTo(ActionPlanSamples.DefaultActions.Count));
    }
}
