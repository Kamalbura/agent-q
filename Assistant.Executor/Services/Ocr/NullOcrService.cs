using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Services;

namespace Assistant.Executor.Services.Ocr;

public sealed class NullOcrService : IOcrService
{
    public Task<OcrResult> ReadScreenAsync(byte[]? screenshotPng, CancellationToken cancellationToken = default)
    {
        var regions = new List<TextRegion>();
        var result = new OcrResult(string.Empty, regions);
        return Task.FromResult(result);
    }
}
