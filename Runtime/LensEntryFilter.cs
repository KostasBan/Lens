using System;

namespace KostasBan.Lens
{
    internal sealed class LensEntryFilter
    {
        public bool MatchesSection(string sectionTitle, string searchText)
        {
            return Contains(sectionTitle, searchText);
        }

        public bool MatchesEntry(LensEntry entry, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            return Contains(entry.Key, searchText) ||
                   Contains(entry.Value, searchText) ||
                   Contains(entry.ActionLabel, searchText);
        }

        private static bool Contains(string value, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
