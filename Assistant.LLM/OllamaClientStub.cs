using Assistant.Core.Samples;
using Serilog;

namespace Assistant.LLM;

public sealed class OllamaClientStub : ILlmClient
{
    private readonly ILogger _logger;

    public OllamaClientStub(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
    }

    public Task<string> GetActionPlanAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _logger.Information("Stub LLM invoked with prompt length {Length}", prompt.Length);
        return Task.FromResult(ActionPlanSamples.DefaultPlanJson);
    }
}
