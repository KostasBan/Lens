using System.Collections.Generic;

namespace KostasBan.Lens
{
    public interface ILensSectionProvider
    {
        string SectionTitle { get; }

        IEnumerable<LensEntry> GetEntries();
    }
}
