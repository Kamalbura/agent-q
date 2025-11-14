using System.Threading;
using System.Threading.Tasks;
using Assistant.Core.Context;

namespace Assistant.Core.Services.Vision;

public interface IScreenContextCollector
{
    Task<ScreenContextDto> CaptureContextAsync(CancellationToken cancellationToken = default);
}
