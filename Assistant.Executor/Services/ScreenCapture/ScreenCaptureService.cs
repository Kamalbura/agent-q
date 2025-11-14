using System.Drawing;
using System.Windows.Forms;
using Assistant.Core.Services.Vision;

namespace Assistant.Executor.Services.ScreenCapture
{
    public class ScreenCaptureService : IScreenCaptureService
    {
        public Bitmap CapturePrimaryScreen()
        {
            var screen = Screen.PrimaryScreen ?? throw new InvalidOperationException("Primary screen unavailable.");
            var bmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, bmp.Size);
            }
            return bmp;
        }
    }
}
