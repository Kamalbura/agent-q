using System.Drawing;
using Assistant.Core.Services.Vision;

namespace Assistant.Executor.Services.Diff
{
    public class ImageDiffEngine : IDiffEngine
    {
        /// <summary>
        /// Very small, testable pixel-by-pixel comparison that computes the fraction of differing pixels.
        /// Returns 0.0 for identical images and 1.0 for fully different.
        /// </summary>
        public double Compare(Bitmap a, Bitmap b)
        {
            if (a == null || b == null) return 1.0;
            if (a.Width != b.Width || a.Height != b.Height) return 1.0;

            long diff = 0;
            long total = (long)a.Width * a.Height;

            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                {
                    var pa = a.GetPixel(x, y);
                    var pb = b.GetPixel(x, y);
                    if (pa != pb) diff++;
                }
            }

            return (double)diff / total;
        }
    }
}
