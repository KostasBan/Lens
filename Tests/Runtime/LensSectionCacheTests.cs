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
            var provider = new CountingProvider("Collapsed", "Collapsed", new LensEntry("Hidden", "Value"));
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
            var provider = new CountingProvider("Feature Flags", "Feature Flags", new LensEntry("shop_v2", "Enabled"));
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
            var provider = new CountingProvider("Feature Flags", "Feature Flags", new LensEntry("shop_v2", "Enabled"));
            var providers = new[] { provider };

            state.ToggleSection("Feature Flags");

            cache.RefreshIfNeeded(providers, state, filter, "shop", 0f, 0.25f);

            Assert.AreEqual(1, provider.ReadCount);
            Assert.AreEqual(1, cache.Sections.Count);
            Assert.AreEqual(1, cache.Sections[0].VisibleEntries.Count);
        }

        [Test]
        public void DuplicateSectionTitlesUseIndependentProviderIdentity()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var first = new CountingProvider("Flags", new LensEntry("First", "1"));
            var second = new CountingProvider("Flags", new LensEntry("Second", "2"));

            cache.RefreshIfNeeded(new[] { first, second }, state, filter, string.Empty, 0f, 0.25f);

            Assert.AreNotEqual(cache.Sections[0].Identity, cache.Sections[1].Identity);

            state.ToggleSection(cache.Sections[0].Identity);
            cache.RequestRefresh();
            cache.RefreshIfNeeded(new[] { first, second }, state, filter, string.Empty, 0.1f, 0.25f);

            Assert.IsFalse(state.IsSectionExpanded(cache.Sections[0].Identity));
            Assert.IsTrue(state.IsSectionExpanded(cache.Sections[1].Identity));
            Assert.AreEqual(0, cache.Sections[0].VisibleEntries.Count);
            Assert.AreEqual(1, cache.Sections[1].VisibleEntries.Count);
        }

        [Test]
        public void IdentifiedProviderUsesStableSectionId()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new IdentifiedProvider("Flags", "game.flags", new LensEntry("Feature", "Enabled"));

            cache.RefreshIfNeeded(new[] { provider }, state, filter, string.Empty, 0f, 0.25f);

            Assert.AreEqual("game.flags", cache.Sections[0].Identity);

            state.ToggleSection("game.flags");
            cache.RequestRefresh();
            cache.RefreshIfNeeded(new[] { provider }, state, filter, string.Empty, 0.1f, 0.25f);

            Assert.IsFalse(state.IsSectionExpanded("game.flags"));
            Assert.AreEqual(0, cache.Sections[0].VisibleEntries.Count);
        }

        [Test]
        public void EmptySectionIdFallsBackToProviderIdentity()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var first = new IdentifiedProvider("Flags", string.Empty, new LensEntry("First", "1"));
            var second = new IdentifiedProvider("Flags", "   ", new LensEntry("Second", "2"));

            cache.RefreshIfNeeded(new[] { first, second }, state, filter, string.Empty, 0f, 0.25f);

            Assert.AreNotEqual(string.Empty, cache.Sections[0].Identity);
            Assert.AreNotEqual(string.Empty, cache.Sections[1].Identity);
            Assert.AreNotEqual(cache.Sections[0].Identity, cache.Sections[1].Identity);
        }

        [Test]
        public void ProviderFailuresBubbleDuringRefresh()
        {
            var cache = new LensSectionCache();
            var state = new LensConsoleState();
            var provider = new ThrowingProvider();

            Assert.Throws<InvalidOperationException>(() => cache.RefreshIfNeeded(new[] { provider }, state, filter, string.Empty, 0f, 0.25f));
        }

        private sealed class CountingProvider : ILensIdentifiedSectionProvider
        {
            private readonly LensEntry[] entries;

            public CountingProvider(string sectionTitle, params LensEntry[] entries)
                : this(sectionTitle, null, entries)
            {
            }

            public CountingProvider(string sectionTitle, string sectionId, params LensEntry[] entries)
            {
                SectionTitle = sectionTitle;
                SectionId = sectionId;
                this.entries = entries;
            }

            public string SectionTitle { get; }

            public string SectionId { get; }

            public int ReadCount { get; private set; }

            public IEnumerable<LensEntry> GetEntries()
            {
                ReadCount++;
                return entries;
            }
        }

        private sealed class IdentifiedProvider : ILensIdentifiedSectionProvider
        {
            private readonly LensEntry[] entries;

            public IdentifiedProvider(string sectionTitle, string sectionId, params LensEntry[] entries)
            {
                SectionTitle = sectionTitle;
                SectionId = sectionId;
                this.entries = entries;
            }

            public string SectionTitle { get; }

            public string SectionId { get; }

            public IEnumerable<LensEntry> GetEntries()
            {
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
