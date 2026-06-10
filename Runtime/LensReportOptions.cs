namespace KostasBan.Lens
{
    public readonly struct LensReportOptions
    {
        public LensReportOptions(bool prettyJson, bool includeScreenshot, string screenshotFileNamePrefix = null)
        {
            PrettyJson = prettyJson;
            IncludeScreenshot = includeScreenshot;
            ScreenshotFileNamePrefix = screenshotFileNamePrefix ?? string.Empty;
        }

        public bool PrettyJson { get; }

        public bool IncludeScreenshot { get; }

        public string ScreenshotFileNamePrefix { get; }

        public static LensReportOptions Default => new LensReportOptions(true, false);
    }
}
