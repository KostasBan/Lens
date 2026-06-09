namespace KostasBan.Lens
{
    public readonly struct LensEntry
    {
        public LensEntry(string key, string value)
        {
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string Key { get; }

        public string Value { get; }
    }
}
