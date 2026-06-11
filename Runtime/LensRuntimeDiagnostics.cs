namespace KostasBan.Lens
{
    internal readonly struct LensRuntimeDiagnostics
    {
        public LensRuntimeDiagnostics(float lastRefreshTime, int providerCount, int visibleEntryCount, int refreshedEntryCount)
        {
            LastRefreshTime = lastRefreshTime;
            ProviderCount = providerCount;
            VisibleEntryCount = visibleEntryCount;
            RefreshedEntryCount = refreshedEntryCount;
        }

        public float LastRefreshTime { get; }

        public int ProviderCount { get; }

        public int VisibleEntryCount { get; }

        public int RefreshedEntryCount { get; }
    }
}
