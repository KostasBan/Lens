using NUnit.Framework;

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
        public void ClearRemovesAllProviders()
        {
            LensSectionRegistry.Register(new StaticProvider("A", new LensEntry("A", "1")));
            LensSectionRegistry.Register(new StaticProvider("B", new LensEntry("B", "2")));

            LensSectionRegistry.Clear();

            Assert.AreEqual(0, LensSectionRegistry.Providers.Count);
        }
    }
}
