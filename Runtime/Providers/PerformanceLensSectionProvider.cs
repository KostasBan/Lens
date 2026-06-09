using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class PerformanceLensSectionProvider : ILensSectionProvider
    {
        public string SectionTitle => "Performance";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            var deltaTime = Time.unscaledDeltaTime;
            var fps = deltaTime > 0f ? 1f / deltaTime : 0f;

            yield return new LensEntry("FPS", fps.ToString("0"));
            yield return new LensEntry("Frame Time", (deltaTime * 1000f).ToString("0.0 ms"));
        }
    }
}
