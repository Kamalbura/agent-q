using Assistant.Executor.Abstractions;
using WindowsInput;

namespace Assistant.Executor.Input;

public sealed class InputSimulatorTextEntry : ITextEntrySimulator
{
    public void EnterText(string text)
    {
        Simulate.Events()
            .Click(text)
            .Invoke()
            .GetAwaiter()
            .GetResult();
    }
}
