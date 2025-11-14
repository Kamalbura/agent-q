using System.Threading.Tasks;
using NUnit.Framework;
using System;
using Assistant.Core.Context;
using Assistant.Core.Samples;
using Assistant.Executor;
using Assistant.Executor.Abstractions;

namespace Assistant.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class EndToEndIntegrationTests
    {
        [Test]
        public async Task Execute_DefaultPlan_NoExceptionsAndActionsDelegated()
        {
            // Arrange
            var typed = new System.Collections.Generic.List<string>();
            var launched = new System.Collections.Generic.List<string>();

            var typer = new FakeTextEntrySimulator(text => typed.Add(text));
            var launcher = new FakeProcessLauncher(cmd => launched.Add(cmd));

            var executor = new ActionExecutor(typer, launcher);

            // Act
            await executor.ExecutePlanAsync(ActionPlanSamples.DefaultActions, CreateContext());

            // Assert
            Assert.IsNotEmpty(typed, "Expected some text to have been typed by the executor.");
            Assert.IsNotEmpty(launched, "Expected at least one application to be launched.");
        }

        private class FakeTextEntrySimulator : ITextEntrySimulator
        {
            private readonly System.Action<string> _onText;
            public FakeTextEntrySimulator(System.Action<string> onText) => _onText = onText;
            public void EnterText(string text) => _onText?.Invoke(text);
        }

        private class FakeProcessLauncher : IProcessLauncher
        {
            private readonly System.Action<string> _onLaunch;
            public FakeProcessLauncher(System.Action<string> onLaunch) => _onLaunch = onLaunch;
            public void Launch(string command) => _onLaunch?.Invoke(command);
        }

        private static ScreenContextDto CreateContext() =>
            new(
                "test.png",
                "hash",
                Array.Empty<byte>(),
                string.Empty,
                Array.Empty<UiElementDto>(),
                "Window",
                new Rect(0, 0, 1920, 1080),
                DateTime.UtcNow,
                null);
    }
}
