using System.Drawing;

namespace Assistant.Core.Services.Vision
{
    public interface IDiffEngine
    {
        /// <summary>
        /// Compare two bitmaps and return a similarity score between 0.0 (identical) and 1.0 (completely different).
        /// </summary>
        double Compare(Bitmap a, Bitmap b);
    }
}
