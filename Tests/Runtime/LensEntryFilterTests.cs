using NUnit.Framework;

namespace KostasBan.Lens.Tests
{
    public sealed class LensEntryFilterTests
    {
        [Test]
        public void MatchesSectionTitleCaseInsensitively()
        {
            var filter = new LensEntryFilter();

            Assert.IsTrue(filter.MatchesSection("Feature Flags", "flags"));
        }

        [Test]
        public void MatchesEntryKeyValueAndActionLabel()
        {
            var filter = new LensEntryFilter();
            var readOnly = new LensEntry("Scene", "SampleScene");
            var action = LensEntry.Button("Debug", "Open Console", () => { });

            Assert.IsTrue(filter.MatchesEntry(readOnly, "scene"));
            Assert.IsTrue(filter.MatchesEntry(readOnly, "sample"));
            Assert.IsTrue(filter.MatchesEntry(action, "console"));
            Assert.IsFalse(filter.MatchesEntry(readOnly, "missing"));
        }
    }
}
