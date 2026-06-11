namespace KostasBan.Lens
{
    public readonly struct LensReportArtifact
    {
        public LensReportArtifact(string textPath, string jsonPath, string screenshotPath)
        {
            TextPath = textPath ?? string.Empty;
            JsonPath = jsonPath ?? string.Empty;
            ScreenshotPath = screenshotPath ?? string.Empty;
        }

        public string TextPath { get; }

        public string JsonPath { get; }

        public string ScreenshotPath { get; }

        public bool HasTextPath => !string.IsNullOrWhiteSpace(TextPath);

        public bool HasJsonPath => !string.IsNullOrWhiteSpace(JsonPath);

        public bool HasScreenshotPath => !string.IsNullOrWhiteSpace(ScreenshotPath);
    }
}
