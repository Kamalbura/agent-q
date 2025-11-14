namespace Assistant.LLM;

public interface ILlmClient
{
    Task<string> GetActionPlanAsync(string prompt, CancellationToken cancellationToken = default);
}
