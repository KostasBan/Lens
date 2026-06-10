using System.Collections.Generic;
using KostasBan.Lens;
using UnityEngine;

namespace KostasBan.Lens.Samples
{
    public sealed class FakeRemoteConfigLensSectionProvider : ILensSectionProvider
    {
        private static readonly LensOption<string>[] EnvironmentOptions =
        {
            new LensOption<string>("dev", "Development"),
            new LensOption<string>("stage", "Staging"),
            new LensOption<string>("prod", "Production")
        };

        private static readonly LensOption<string>[] ExperimentOptions =
        {
            new LensOption<string>("tutorial", "Tutorial"),
            new LensOption<string>("shop", "Shop"),
            new LensOption<string>("economy", "Economy")
        };

        private bool remoteConfigReady = true;
        private string environment = "stage";
        private readonly List<string> activeExperiments = new List<string> { "tutorial", "shop" };
        private float rolloutPercent = 35f;
        private string configVersion = "fake-config-42";
        private string sampleApiKey = "fake-key-do-not-use";

        public string SectionTitle => "Cookbook Remote Config";

        public IEnumerable<LensEntry> GetEntries()
        {
            yield return LensEntry.Toggle("Remote Config Ready", () => remoteConfigReady, value => remoteConfigReady = value);
            yield return LensEntry.SingleSelect("Environment", () => environment, value => environment = value, EnvironmentOptions);
            yield return LensEntry.MultiSelect("Active Experiments", () => activeExperiments, values =>
            {
                activeExperiments.Clear();
                activeExperiments.AddRange(values);
            }, ExperimentOptions);
            yield return LensEntry.Slider("Rollout Percent", () => rolloutPercent, value => rolloutPercent = Mathf.Clamp(value, 0f, 100f), 0f, 100f, 5f, "0'%'");
            yield return LensEntry.Text("Config Version", () => configVersion, value => configVersion = value);
            yield return LensEntry.Text("Sample API Key", () => sampleApiKey, value => sampleApiKey = value, true, infoText: "Demonstrates redaction. Do not expose real API keys.");
        }
    }
}
