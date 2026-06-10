using System.Collections.Generic;
using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class SampleFeatureFlagsLensSectionProvider : ILensSectionProvider
    {
        private static readonly LensOption<string>[] EnvironmentOptions =
        {
            new LensOption<string>("Development", "Development"),
            new LensOption<string>("Staging", "Staging"),
            new LensOption<string>("Production", "Production")
        };

        private static readonly LensOption<string>[] RewardOptions =
        {
            new LensOption<string>("coins", "Coins"),
            new LensOption<string>("gems", "Gems"),
            new LensOption<string>("boosters", "Boosters")
        };

        private bool newTutorial = true;
        private bool shopV2;
        private bool adaptiveDifficulty = true;
        private bool sampleContentUnlocked;
        private bool fakeDebugPanelOpen;
        private string environmentLabel = "Development";
        private string sampleUserToken = "sample-token-12345";
        private float xpMultiplier = 1.25f;
        private float rolloutPercent = 35f;
        private string activeEnvironment = "Development";
        private readonly List<string> enabledRewards = new List<string> { "coins", "gems" };
        private float contentDownloadProgress = 72f;

        public string SectionTitle => "Sample Feature Flags";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            yield return LensEntry.Toggle("new_tutorial", () => newTutorial, value => newTutorial = value, infoText: "Example boolean feature flag controlled by a provider callback.");
            yield return LensEntry.Toggle("shop_v2", () => shopV2, value => shopV2 = value);
            yield return LensEntry.Toggle("adaptive_difficulty", () => adaptiveDifficulty, value => adaptiveDifficulty = value);
            yield return LensEntry.Text("Environment Label", () => environmentLabel, value => environmentLabel = value);
            yield return LensEntry.Text("Sample User Token", () => sampleUserToken, value => sampleUserToken = value, true);
            yield return LensEntry.Number("XP Multiplier", () => xpMultiplier, value => xpMultiplier = Mathf.Clamp(value, 0f, 10f));
            yield return LensEntry.Slider("Rollout Percent", () => rolloutPercent, value => rolloutPercent = Mathf.Clamp(value, 0f, 100f), 0f, 100f, 5f, "0'%'", infoText: "Drafts locally and commits only when Apply is clicked.");
            yield return LensEntry.SingleSelect("Active Environment", () => activeEnvironment, value => activeEnvironment = value, EnvironmentOptions);
            yield return LensEntry.MultiSelect("Enabled Rewards", () => enabledRewards, values =>
            {
                enabledRewards.Clear();
                enabledRewards.AddRange(values);
            }, RewardOptions);
            yield return LensEntry.Progress("Content Download", () => contentDownloadProgress, () => 100f, "Catalog");
            yield return LensEntry.Button("Unlock Sample Content", () => sampleContentUnlocked = true, true, "Unlock all sample content?");
            yield return LensEntry.Button("Toggle Fake Debug Panel", () => fakeDebugPanelOpen = !fakeDebugPanelOpen);
            yield return new LensEntry("Sample Content Unlocked", sampleContentUnlocked.ToString());
            yield return new LensEntry("Fake Debug Panel", fakeDebugPanelOpen ? "Open" : "Closed");
        }
    }
}
