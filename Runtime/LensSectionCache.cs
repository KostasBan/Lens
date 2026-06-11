using System;
using System.Collections.Generic;

namespace KostasBan.Lens
{
    internal sealed class LensSectionCache
    {
        private readonly List<LensCachedSection> sections = new List<LensCachedSection>();
        private readonly List<LensCachedSection> nextSections = new List<LensCachedSection>();

        private bool refreshRequested = true;
        private float lastRefreshTime = float.NegativeInfinity;

        public IReadOnlyList<LensCachedSection> Sections => sections;

        public LensRuntimeDiagnostics Diagnostics { get; private set; }

        public void RequestRefresh()
        {
            refreshRequested = true;
        }

        public void RefreshIfNeeded(
            IReadOnlyList<ILensSectionProvider> providers,
            LensConsoleState state,
            LensEntryFilter filter,
            string searchText,
            float now,
            float refreshIntervalSeconds)
        {
            if (!ShouldRefresh(now, refreshIntervalSeconds))
            {
                return;
            }

            Refresh(providers, state, filter, searchText, now, false);
        }

        public void ForceRefresh(
            IReadOnlyList<ILensSectionProvider> providers,
            LensConsoleState state,
            LensEntryFilter filter,
            string searchText,
            float now,
            bool includeCollapsedSections)
        {
            Refresh(providers, state, filter, searchText, now, includeCollapsedSections);
        }

        private bool ShouldRefresh(float now, float refreshIntervalSeconds)
        {
            if (refreshRequested)
            {
                return true;
            }

            if (refreshIntervalSeconds <= 0f)
            {
                return true;
            }

            return now - lastRefreshTime >= refreshIntervalSeconds;
        }

        private void Refresh(
            IReadOnlyList<ILensSectionProvider> providers,
            LensConsoleState state,
            LensEntryFilter filter,
            string searchText,
            float now,
            bool includeCollapsedSections)
        {
            nextSections.Clear();

            var hasSearch = !string.IsNullOrWhiteSpace(searchText);
            var visibleEntryCount = 0;
            var refreshedEntryCount = 0;

            if (providers != null)
            {
                for (var i = 0; i < providers.Count; i++)
                {
                    var provider = providers[i];
                    if (provider == null)
                    {
                        continue;
                    }

                    var section = FindExistingSection(provider) ?? new LensCachedSection(provider);
                    section.RefreshTitle();
                    var expanded = hasSearch || state.IsSectionExpanded(section.Title);
                    var shouldFetchEntries = hasSearch || expanded || includeCollapsedSections;

                    if (shouldFetchEntries)
                    {
                        section.RefreshEntries();
                        refreshedEntryCount += section.Entries.Count;
                    }

                    section.UpdateVisibleEntries(filter, searchText, expanded, hasSearch);

                    if (!hasSearch || section.VisibleEntries.Count > 0)
                    {
                        visibleEntryCount += expanded ? section.VisibleEntries.Count : 0;
                        nextSections.Add(section);
                    }
                }
            }

            sections.Clear();
            sections.AddRange(nextSections);
            lastRefreshTime = now;
            refreshRequested = false;
            Diagnostics = new LensRuntimeDiagnostics(now, providers != null ? providers.Count : 0, visibleEntryCount, refreshedEntryCount);
        }

        private LensCachedSection FindExistingSection(ILensSectionProvider provider)
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (ReferenceEquals(sections[i].Provider, provider))
                {
                    return sections[i];
                }
            }

            return null;
        }
    }

    internal sealed class LensCachedSection
    {
        private string headerLabel;
        private bool headerExpanded;
        private int headerCount = -1;

        public LensCachedSection(ILensSectionProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            RefreshTitle();
        }

        public ILensSectionProvider Provider { get; }

        public string Title { get; private set; }

        public List<LensEntry> Entries { get; } = new List<LensEntry>();

        public List<LensEntry> VisibleEntries { get; } = new List<LensEntry>();

        public int DisplayEntryCount => Entries.Count;

        public void RefreshTitle()
        {
            var nextTitle = string.IsNullOrWhiteSpace(Provider.SectionTitle) ? "Untitled" : Provider.SectionTitle;
            if (string.Equals(Title, nextTitle, StringComparison.Ordinal))
            {
                return;
            }

            Title = nextTitle;
            headerLabel = null;
        }

        public void RefreshEntries()
        {
            Entries.Clear();

            foreach (var entry in Provider.GetEntries())
            {
                Entries.Add(entry);
            }
        }

        public void UpdateVisibleEntries(LensEntryFilter filter, string searchText, bool expanded, bool hasSearch)
        {
            VisibleEntries.Clear();

            if (!expanded)
            {
                return;
            }

            var sectionMatchesSearch = hasSearch && filter.MatchesSection(Title, searchText);

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (!hasSearch || sectionMatchesSearch || filter.MatchesEntry(entry, searchText))
                {
                    VisibleEntries.Add(entry);
                }
            }
        }

        public string GetHeaderLabel(bool expanded)
        {
            var count = expanded ? VisibleEntries.Count : DisplayEntryCount;
            if (headerLabel != null && headerExpanded == expanded && headerCount == count)
            {
                return headerLabel;
            }

            headerExpanded = expanded;
            headerCount = count;
            headerLabel = string.Concat(expanded ? "[-] " : "[+] ", Title, " (", count.ToString(), ")");
            return headerLabel;
        }
    }
}
