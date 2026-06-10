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

        [Test]
        public void SensitiveEntryDoesNotMatchRawValue()
        {
            var filter = new LensEntryFilter();
            var entry = new LensEntry("Token", "secret-token", true);

            Assert.IsTrue(filter.MatchesEntry(entry, "token"));
            Assert.IsTrue(filter.MatchesEntry(entry, "redacted"));
            Assert.IsFalse(filter.MatchesEntry(entry, "secret-token"));
        }

        [Test]
        public void MatchesEntryInfoTextAndCustomSearchText()
        {
            var filter = new LensEntryFilter();
            var info = LensEntry.ReadOnly("Rollout", "35%", "Percentage of players in the experiment.");
            var custom = LensEntry.Custom("Custom", "sample", new { Id = 7 }, _ => "Visible", _ => "Hidden Search Text");

            Assert.IsTrue(filter.MatchesEntry(info, "experiment"));
            Assert.IsTrue(filter.MatchesEntry(custom, "hidden"));
        }
    }
}
