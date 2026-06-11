using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace KostasBan.Lens.Tests
{
    public sealed class LensSectionCacheTests
    {
        private readonly LensEntryFilter filter = new LensEntryFilter();

        [Test]
        public void RefreshUsesIntervalWhenNotRequested()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new CountingProvider("Build", new LensEntry("Version", "1"));
            var providers = new[] { provider };

            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0f, 0.25f);
            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0.1f, 0.25f);
            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0.3f, 0.25f);

            Assert.AreEqual(2, provider.ReadCount);
            Assert.AreEqual(1, cache.Sections.Count);
            Assert.AreEqual(1, cache.Sections[0].VisibleEntries.Count);
            Assert.AreEqual(1, cache.Diagnostics.VisibleEntryCount);
            Assert.AreEqual(1, cache.Diagnostics.RefreshedEntryCount);
        }

        [Test]
        public void CachedEntriesUpdateOnlyAfterIntervalElapses()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new MutableProvider();
            var providers = new[] { provider };

            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0f, 0.25f);
            provider.Value = "2";
            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0.1f, 0.25f);

            Assert.AreEqual("1", cache.Sections[0].VisibleEntries[0].Value);

            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0.3f, 0.25f);

            Assert.AreEqual("2", cache.Sections[0].VisibleEntries[0].Value);
        }

        [Test]
        public void CollapsedSectionDoesNotEnumerateEntriesWithoutSearch()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new CountingProvider("Collapsed", new LensEntry("Hidden", "Value"));
            var providers = new[] { provider };

            state.ToggleSection("Collapsed");

            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0f, 0.25f);

            Assert.AreEqual(0, provider.ReadCount);
            Assert.AreEqual(1, cache.Sections.Count);
            Assert.AreEqual(0, cache.Sections[0].DisplayEntryCount);
            Assert.AreEqual(0, cache.Diagnostics.RefreshedEntryCount);
        }

        [Test]
        public void ExpandingCollapsedSectionEnumeratesEntriesAfterRefreshRequest()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new CountingProvider("Feature Flags", new LensEntry("shop_v2", "Enabled"));
            var providers = new[] { provider };

            state.ToggleSection("Feature Flags");
            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0f, 0.25f);

            state.ToggleSection("Feature Flags");
            cache.RequestRefresh();
            cache.RefreshIfNeeded(providers, state, filter, string.Empty, 0.1f, 0.25f);

            Assert.AreEqual(1, provider.ReadCount);
            Assert.AreEqual(1, cache.Sections[0].VisibleEntries.Count);
        }

        [Test]
        public void SearchEnumeratesCollapsedSectionsForFiltering()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new CountingProvider("Feature Flags", new LensEntry("shop_v2", "Enabled"));
            var providers = new[] { provider };

            state.ToggleSection("Feature Flags");

            cache.RefreshIfNeeded(providers, state, filter, "shop", 0f, 0.25f);

            Assert.AreEqual(1, provider.ReadCount);
            Assert.AreEqual(1, cache.Sections.Count);
            Assert.AreEqual(1, cache.Sections[0].VisibleEntries.Count);
        }

        [Test]
        public void ProviderFailuresBubbleDuringRefresh()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new ThrowingProvider();

            Assert.Throws<InvalidOperationException>(() => cache.RefreshIfNeeded(new[] { provider }, state, filter, string.Empty, 0f, 0.25f));
        }

        private sealed class CountingProvider : ILensSectionProvider
        {
            private readonly LensEntry[] entries;

            public CountingProvider(string sectionTitle, params LensEntry[] entries)
            {
                SectionTitle = sectionTitle;
                this.entries = entries;
            }

            public string SectionTitle { get; }

            public int ReadCount { get; private set; }

            public IEnumerable<LensEntry> GetEntries()
            {
                ReadCount++;
                return entries;
            }
        }

        private sealed class ThrowingProvider : ILensSectionProvider
        {
            public string SectionTitle => "Broken";

            public IEnumerable<LensEntry> GetEntries()
            {
                throw new InvalidOperationException("Broken provider.");
            }
        }

        private sealed class MutableProvider : ILensSectionProvider
        {
            public string SectionTitle => "Build";

            public string Value { get; set; } = "1";

            public IEnumerable<LensEntry> GetEntries()
            {
                yield return new LensEntry("Version", Value);
            }
        }
    }
}
