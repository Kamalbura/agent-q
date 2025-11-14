using System.Drawing;

namespace Assistant.Core.Services.Vision
{
    public interface IScreenCaptureService
    {
        /// <summary>
        /// Capture a screenshot of the primary display and return as a bitmap.
        /// </summary>
        /// <returns>Bitmap snapshot of the primary screen.</returns>
        Bitmap CapturePrimaryScreen();
    }
}
