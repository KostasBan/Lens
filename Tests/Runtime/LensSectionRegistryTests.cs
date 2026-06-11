using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensSectionRegistryTests
    {
        [TearDown]
        public void TearDown()
        {
            LensSectionRegistry.Clear();
        }

        [Test]
        public void RegisterAddsProviderOnlyOnce()
        {
            var provider = new StaticProvider("Test", new LensEntry("Key", "Value"));

            LensSectionRegistry.Register(provider);
            LensSectionRegistry.Register(provider);

            Assert.AreEqual(1, LensSectionRegistry.Providers.Count);
        }

        [Test]
        public void UnregisterRemovesProvider()
        {
            var provider = new StaticProvider("Test", new LensEntry("Key", "Value"));

            LensSectionRegistry.Register(provider);
            LensSectionRegistry.Unregister(provider);

            Assert.AreEqual(0, LensSectionRegistry.Providers.Count);
        }

        [Test]
        public void RegisterAndUnregisterRejectNullProviders()
        {
            Assert.Throws<System.ArgumentNullException>(() => LensSectionRegistry.Register(null));
            Assert.Throws<System.ArgumentNullException>(() => LensSectionRegistry.Unregister(null));
        }

        [Test]
        public void ClearRemovesAllProviders()
        {
            LensSectionRegistry.Register(new StaticProvider("A", new LensEntry("A", "1")));
            LensSectionRegistry.Register(new StaticProvider("B", new LensEntry("B", "2")));

            LensSectionRegistry.Clear();

            Assert.AreEqual(0, LensSectionRegistry.Providers.Count);
        }

        [Test]
        public void ResetForPlayModeClearsStaticRuntimeState()
        {
            LensSectionRegistry.Register(new StaticProvider("A", new LensEntry("A", "1")));
            LensEntryDrawerRegistry.Register("sample", new NoopDrawer());
            LensRuntimePolicy.SetAllowed(false);
            LensReportMetadata.ProjectBuildNumber = "123";

            LensRuntimeState.ResetForPlayMode();

            Assert.AreEqual(0, LensSectionRegistry.Providers.Count);
            Assert.IsFalse(LensEntryDrawerRegistry.TryGet("sample", out _));
            Assert.AreEqual(LensRuntimePolicy.DefaultIsAllowed, LensRuntimePolicy.IsAllowed);
            Assert.AreEqual(string.Empty, LensReportMetadata.ProjectBuildNumber);
        }

        [Test]
        public void LensSectionBehaviourRegistersAndUnregistersWithOwnerLifetime()
        {
            var gameObject = new GameObject("Lens Section Behaviour Test");

            try
            {
                var section = gameObject.AddComponent<TestLensSectionBehaviour>();

                Assert.AreEqual(1, LensSectionRegistry.Providers.Count);
                Assert.AreSame(section, LensSectionRegistry.Providers[0]);

                gameObject.SetActive(false);

                Assert.AreEqual(0, LensSectionRegistry.Providers.Count);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void LensSectionBehaviourDoesNotDoubleRegisterAcrossSceneStyleReload()
        {
            var gameObject = new GameObject("Lens Section Behaviour Reload Test");

            try
            {
                gameObject.AddComponent<TestLensSectionBehaviour>();

                gameObject.SetActive(false);
                gameObject.SetActive(true);
                gameObject.SetActive(false);
                gameObject.SetActive(true);

                Assert.AreEqual(1, LensSectionRegistry.Providers.Count);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class TestLensSectionBehaviour : LensSectionBehaviour
        {
            public override string SectionTitle => "Behaviour";

            public override System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
            {
                yield return new LensEntry("Key", "Value");
            }
        }

        private sealed class NoopDrawer : ILensEntryDrawer
        {
            public void Draw(LensEntryDrawContext context, LensEntry entry)
            {
            }
        }
    }
}
