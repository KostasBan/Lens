using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensRuntimeConsoleTests
    {
        [TearDown]
        public void TearDown()
        {
            var consoles = Object.FindObjectsByType<LensRuntimeConsole>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < consoles.Length; i++)
            {
                if (consoles[i] != null)
                {
                    Object.DestroyImmediate(consoles[i].gameObject);
                }
            }
        }

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

        [Test]
        public void TryFindExistingReturnsFalseWhenNoConsoleExists()
        {
            Assert.IsFalse(LensRuntimeConsole.TryFindExisting(out var console));
            Assert.IsNull(console);
        }

        [Test]
        public void EnsureExistsCreatesConsoleWhenMissing()
        {
            var console = LensRuntimeConsole.EnsureExists("Lens Test Console");

            Assert.IsNotNull(console);
            Assert.AreEqual("Lens Test Console", console.gameObject.name);
        }

        [Test]
        public void EnsureExistsReturnsExistingConsole()
        {
            var first = LensRuntimeConsole.EnsureExists("First Lens Console");
            var second = LensRuntimeConsole.EnsureExists("Second Lens Console");

            Assert.AreSame(first, second);
            Assert.AreEqual("First Lens Console", second.gameObject.name);
        }

        [Test]
        public void TryFindExistingReturnsExistingConsole()
        {
            var expected = LensRuntimeConsole.EnsureExists();

            Assert.IsTrue(LensRuntimeConsole.TryFindExisting(out var actual));
            Assert.AreSame(expected, actual);
        }
    }
}
