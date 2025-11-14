using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Context;

namespace Assistant.Core.Services.Vision
{
    /// <summary>
    /// Adapter that accepts structured screen context (OCR text + UI tree) and returns a JSON plan or reasoning.
    /// </summary>
    public interface IVisionLlmAdapter
    {
        Task<string> GetActionPlanAsync(string intent, ScreenContextDto context, CancellationToken cancellationToken = default);
    }
}
