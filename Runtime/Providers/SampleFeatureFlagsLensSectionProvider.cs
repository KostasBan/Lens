namespace KostasBan.Lens
{
    public sealed class SampleFeatureFlagsLensSectionProvider : ILensSectionProvider
    {
        public string SectionTitle => "Sample Feature Flags";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            yield return new LensEntry("new_tutorial", "true");
            yield return new LensEntry("shop_v2", "false");
            yield return new LensEntry("adaptive_difficulty", "true");
        }
    }
}
