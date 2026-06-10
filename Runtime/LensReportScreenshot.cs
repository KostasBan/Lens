namespace KostasBan.Lens
{
    public readonly struct LensReportScreenshot
    {
        public LensReportScreenshot(string path)
        {
            Path = path ?? string.Empty;
        }

        public string Path { get; }

        public bool HasPath => !string.IsNullOrWhiteSpace(Path);
    }
}
