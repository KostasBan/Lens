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
        private readonly LensEntry newTutorialEntry;
        private readonly LensEntry shopV2Entry;
        private readonly LensEntry adaptiveDifficultyEntry;
        private readonly LensEntry environmentLabelEntry;
        private readonly LensEntry sampleUserTokenEntry;
        private readonly LensEntry xpMultiplierEntry;
        private readonly LensEntry rolloutPercentEntry;
        private readonly LensEntry activeEnvironmentEntry;
        private readonly LensEntry enabledRewardsEntry;
        private readonly LensEntry contentDownloadEntry;
        private readonly LensEntry unlockSampleContentEntry;
        private readonly LensEntry toggleFakeDebugPanelEntry;

        public SampleFeatureFlagsLensSectionProvider()
        {
            newTutorialEntry = LensEntry.Toggle("new_tutorial", () => newTutorial, value => newTutorial = value, infoText: "Example boolean feature flag controlled by a provider callback.");
            shopV2Entry = LensEntry.Toggle("shop_v2", () => shopV2, value => shopV2 = value);
            adaptiveDifficultyEntry = LensEntry.Toggle("adaptive_difficulty", () => adaptiveDifficulty, value => adaptiveDifficulty = value);
            environmentLabelEntry = LensEntry.Text("Environment Label", () => environmentLabel, value => environmentLabel = value);
            sampleUserTokenEntry = LensEntry.Text("Sample User Token", () => sampleUserToken, value => sampleUserToken = value, true);
            xpMultiplierEntry = LensEntry.Number("XP Multiplier", () => xpMultiplier, value => xpMultiplier = Mathf.Clamp(value, 0f, 10f));
            rolloutPercentEntry = LensEntry.Slider("Rollout Percent", () => rolloutPercent, value => rolloutPercent = Mathf.Clamp(value, 0f, 100f), 0f, 100f, 5f, "0'%'", infoText: "Drafts locally and commits only when Apply is clicked.");
            activeEnvironmentEntry = LensEntry.SingleSelect("Active Environment", () => activeEnvironment, value => activeEnvironment = value, EnvironmentOptions);
            enabledRewardsEntry = LensEntry.MultiSelect("Enabled Rewards", () => enabledRewards, values =>
            {
                enabledRewards.Clear();
                enabledRewards.AddRange(values);
            }, RewardOptions);
            contentDownloadEntry = LensEntry.Progress("Content Download", () => contentDownloadProgress, () => 100f, "Catalog");
            unlockSampleContentEntry = LensEntry.Button("Unlock Sample Content", () => sampleContentUnlocked = true, true, "Unlock all sample content?");
            toggleFakeDebugPanelEntry = LensEntry.Button("Toggle Fake Debug Panel", () => fakeDebugPanelOpen = !fakeDebugPanelOpen);
        }

        public string SectionTitle => "Sample Feature Flags";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            yield return newTutorialEntry;
            yield return shopV2Entry;
            yield return adaptiveDifficultyEntry;
            yield return environmentLabelEntry;
            yield return sampleUserTokenEntry;
            yield return xpMultiplierEntry;
            yield return rolloutPercentEntry;
            yield return activeEnvironmentEntry;
            yield return enabledRewardsEntry;
            yield return contentDownloadEntry;
            yield return unlockSampleContentEntry;
            yield return toggleFakeDebugPanelEntry;
            yield return new LensEntry("Sample Content Unlocked", sampleContentUnlocked.ToString());
            yield return new LensEntry("Fake Debug Panel", fakeDebugPanelOpen ? "Open" : "Closed");
        }
    }
}
