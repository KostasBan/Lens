using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensLayoutMetricsTests
    {
        [Test]
        public void DesktopSizedLowDpiScreenStaysAtMinimumScale()
        {
            var scale = LensLayoutMetrics.CalculateScale(1920, 1080, 96f, LensUiScaleMode.Auto, 1f, 1f, 3f);

            Assert.AreEqual(1f, scale);
        }

        [Test]
        public void HighDpiPortraitScreenScalesUp()
        {
            var scale = LensLayoutMetrics.CalculateScale(1080, 2400, 440f, LensUiScaleMode.Auto, 1f, 1f, 3f);

            Assert.Greater(scale, 2f);
            Assert.LessOrEqual(scale, 3f);
        }

        [Test]
        public void UnknownDpiPortraitScreenFallsBackToScreenSize()
        {
            var scale = LensLayoutMetrics.CalculateScale(1080, 2400, 0f, LensUiScaleMode.Auto, 1f, 1f, 3f);

            Assert.AreEqual(2.25f, scale);
        }

        [Test]
        public void FixedScaleUsesConfiguredValue()
        {
            var scale = LensLayoutMetrics.CalculateScale(1080, 2400, 440f, LensUiScaleMode.Fixed, 1.75f, 1f, 3f);

            Assert.AreEqual(1.75f, scale);
        }

        [Test]
        public void NarrowPortraitUsesCompactLayout()
        {
            var metrics = LensLayoutMetrics.FromScreen(1080, 2400, 440f, LensUiScaleMode.Auto, 1f, 1f, 3f);

            Assert.IsTrue(metrics.IsCompact);
            Assert.IsTrue(metrics.IsPortrait);
        }

        [Test]
        public void DesktopLandscapeUsesRegularLayout()
        {
            var metrics = LensLayoutMetrics.FromScreen(1920, 1080, 96f, LensUiScaleMode.Auto, 1f, 1f, 3f);

            Assert.IsFalse(metrics.IsCompact);
        }

        [Test]
        public void FloatingButtonClampsInsideLogicalScreen()
        {
            var metrics = LensLayoutMetrics.FromScreen(1080, 2400, 440f, LensUiScaleMode.Auto, 1f, 1f, 3f);
            var rect = metrics.ClampFloatingButton(new Rect(2000f, 3000f, 10f, 10f));

            Assert.LessOrEqual(rect.xMax, metrics.LogicalScreenWidth - metrics.Margin + 0.001f);
            Assert.LessOrEqual(rect.yMax, metrics.LogicalScreenHeight - metrics.Margin + 0.001f);
            Assert.GreaterOrEqual(rect.width, 96f);
            Assert.GreaterOrEqual(rect.height, 48f);
        }
    }
}
