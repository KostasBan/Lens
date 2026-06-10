namespace KostasBan.Lens
{
    public readonly struct LensOption<T>
    {
        public LensOption(T value, string label)
        {
            Value = value;
            Label = string.IsNullOrEmpty(label) ? "(empty)" : label;
        }

        public T Value { get; }

        public string Label { get; }
    }
}
