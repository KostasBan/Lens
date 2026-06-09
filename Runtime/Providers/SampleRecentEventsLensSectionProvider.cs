namespace KostasBan.Lens
{
    public sealed class SampleRecentEventsLensSectionProvider : ILensSectionProvider
    {
        private readonly string[] events =
        {
            "session_started",
            "sample_scene_loaded",
            "sample_button_clicked"
        };

        public string SectionTitle => "Sample Recent Events";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            for (var i = 0; i < events.Length; i++)
            {
                yield return new LensEntry(i.ToString(), events[i]);
            }
        }
    }
}
