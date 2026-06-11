using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensRuntimeConsoleTests
    {
        [Test]
        public void RefreshIntervalIsClampedToNonNegative()
        {
            var gameObject = new GameObject("Lens Runtime Console Test");
            var console = gameObject.AddComponent<LensRuntimeConsole>();

            try
            {
                console.RefreshIntervalSeconds = -1f;

                Assert.AreEqual(0f, console.RefreshIntervalSeconds);

                console.RefreshIntervalSeconds = 0.5f;

                Assert.AreEqual(0.5f, console.RefreshIntervalSeconds);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
