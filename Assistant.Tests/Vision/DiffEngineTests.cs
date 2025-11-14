using System.Drawing;
using NUnit.Framework;
using Assistant.Executor.Services.Diff;

namespace Assistant.Tests.Vision
{
    [TestFixture]
    public class DiffEngineTests
    {
        [Test]
        public void Compare_IdenticalBitmaps_ReturnsZero()
        {
            var a = new Bitmap(10, 10);
            var b = new Bitmap(10, 10);

            using (var g = Graphics.FromImage(a)) g.Clear(Color.AliceBlue);
            using (var g = Graphics.FromImage(b)) g.Clear(Color.AliceBlue);

            var engine = new ImageDiffEngine();
            var score = engine.Compare(a, b);

            Assert.AreEqual(0.0, score, 1e-9);
        }

        [Test]
        public void Compare_DifferentBitmaps_ReturnsNonZero()
        {
            var a = new Bitmap(5, 5);
            var b = new Bitmap(5, 5);

            using (var g = Graphics.FromImage(a)) g.Clear(Color.Black);
            using (var g = Graphics.FromImage(b)) g.Clear(Color.White);

            var engine = new ImageDiffEngine();
            var score = engine.Compare(a, b);

            Assert.Greater(score, 0.0);
        }
    }
}
