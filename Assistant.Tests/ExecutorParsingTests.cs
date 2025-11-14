using Assistant.Core.Samples;
using Assistant.Executor;
using Assistant.Executor.Abstractions;

namespace Assistant.Tests;

public sealed class ExecutorParsingTests
{
    [Test]
    public void Executor_parses_sample_plan()
    {
        var executor = new ActionExecutor(new NoOpTextEntrySimulator(), new NoOpProcessLauncher());
        var success = executor.TryParseActions(ActionPlanSamples.DefaultPlanJson, out var actions, out var errors);

        Assert.That(success, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(actions, Is.Not.Null);
        Assert.That(actions.Count, Is.EqualTo(ActionPlanSamples.DefaultActions.Count));
    }

    private sealed class NoOpTextEntrySimulator : ITextEntrySimulator
    {
        public void EnterText(string text)
        {
        }
    }

    private sealed class NoOpProcessLauncher : IProcessLauncher
    {
        public void Launch(string target)
        {
        }
    }
}
