using System.Collections.Generic;
using KostasBan.Lens;

namespace KostasBan.Lens.Samples
{
    public sealed class FakeAnalyticsLensSectionProvider : ILensSectionProvider
    {
        private readonly Queue<string> recentEvents = new Queue<string>(new[]
        {
            "app_start",
            "tutorial_step_completed",
            "shop_opened"
        });

        private bool captureEnabled = true;
        private int eventsSent = 3;

        public string SectionTitle => "Cookbook Analytics";

        public IEnumerable<LensEntry> GetEntries()
        {
            yield return LensEntry.Toggle("Capture Enabled", () => captureEnabled, value => captureEnabled = value);
            yield return new LensEntry("Events Sent", eventsSent.ToString());
            yield return LensEntry.Button("Record Fake Event", RecordFakeEvent);

            var index = 1;
            foreach (var eventName in recentEvents)
            {
                yield return new LensEntry($"Recent {index}", eventName);
                index++;
            }
        }

        private void RecordFakeEvent()
        {
            eventsSent++;
            recentEvents.Enqueue($"debug_event_{eventsSent}");

            while (recentEvents.Count > 5)
            {
                recentEvents.Dequeue();
            }
        }
    }
}
