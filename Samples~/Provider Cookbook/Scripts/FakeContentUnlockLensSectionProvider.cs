using System.Collections.Generic;
using KostasBan.Lens;

namespace KostasBan.Lens.Samples
{
    public sealed class FakeContentUnlockLensSectionProvider : ILensSectionProvider
    {
        private bool premiumUnlocked;
        private bool debugPanelOpen;
        private int softCurrency = 250;

        public string SectionTitle => "Cookbook Content Unlocks";

        public IEnumerable<LensEntry> GetEntries()
        {
            yield return new LensEntry("Premium Content", premiumUnlocked ? "Unlocked" : "Locked");
            yield return new LensEntry("Debug Panel", debugPanelOpen ? "Open" : "Closed");
            yield return LensEntry.Number("Soft Currency", () => softCurrency, value => softCurrency = System.Math.Max(0, (int)value), infoText: "Fake local value for cookbook testing.");
            yield return LensEntry.Button("Unlock Premium Content", () => premiumUnlocked = true, true, "Unlock fake premium content?");
            yield return LensEntry.Button("Toggle Debug Panel", () => debugPanelOpen = !debugPanelOpen);
            yield return LensEntry.Button("Reset Cookbook State", ResetState, true, "Reset fake cookbook content state?");
        }

        private void ResetState()
        {
            premiumUnlocked = false;
            debugPanelOpen = false;
            softCurrency = 250;
        }
    }
}
