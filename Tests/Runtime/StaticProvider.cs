using System.Collections.Generic;

namespace KostasBan.Lens.Tests
{
    internal sealed class StaticProvider : ILensSectionProvider
    {
        private readonly LensEntry[] entries;

        public StaticProvider(string sectionTitle, params LensEntry[] entries)
        {
            SectionTitle = sectionTitle;
            this.entries = entries;
        }

        public string SectionTitle { get; }

        public IEnumerable<LensEntry> GetEntries()
        {
            return entries;
        }
    }
}
