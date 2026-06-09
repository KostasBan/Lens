using System;
using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class SampleFeatureFlagsLensSectionProvider : ILensSectionProvider
    {
        private bool newTutorial = true;
        private bool shopV2;
        private bool adaptiveDifficulty = true;
        private bool sampleContentUnlocked;
        private bool fakeDebugPanelOpen;
        private string environmentLabel = "Development";
        private float xpMultiplier = 1.25f;

        public string SectionTitle => "Sample Feature Flags";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            yield return LensEntry.Toggle("new_tutorial", () => newTutorial, value => newTutorial = value);
            yield return LensEntry.Toggle("shop_v2", () => shopV2, value => shopV2 = value);
            yield return LensEntry.Toggle("adaptive_difficulty", () => adaptiveDifficulty, value => adaptiveDifficulty = value);
            yield return LensEntry.Text("Environment Label", () => environmentLabel, value => environmentLabel = value);
            yield return LensEntry.Number("XP Multiplier", () => xpMultiplier, value => xpMultiplier = Mathf.Clamp(value, 0f, 10f));
            yield return LensEntry.Button("Unlock Sample Content", () => sampleContentUnlocked = true);
            yield return LensEntry.Button("Toggle Fake Debug Panel", () => fakeDebugPanelOpen = !fakeDebugPanelOpen);
            yield return LensEntry.Button("Fail Sample Action", () => throw new InvalidOperationException("Sample action failure."));
            yield return new LensEntry("Sample Content Unlocked", sampleContentUnlocked.ToString());
            yield return new LensEntry("Fake Debug Panel", fakeDebugPanelOpen ? "Open" : "Closed");
        }
    }
}
