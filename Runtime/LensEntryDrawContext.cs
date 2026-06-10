using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class LensEntryDrawContext
    {
        private readonly LensConsoleState state;

        internal LensEntryDrawContext(string sectionTitle, string entryId, LensConsoleState state, LensLayoutMetrics metrics)
        {
            SectionTitle = sectionTitle ?? string.Empty;
            EntryId = entryId ?? string.Empty;
            this.state = state;
            UiScale = metrics.UiScale;
            IsCompact = metrics.IsCompact;
            LogicalScreenWidth = metrics.LogicalScreenWidth;
            LogicalScreenHeight = metrics.LogicalScreenHeight;
        }

        public string SectionTitle { get; }

        public string EntryId { get; }

        public float UiScale { get; }

        public bool IsCompact { get; }

        public float LogicalScreenWidth { get; }

        public float LogicalScreenHeight { get; }

        public void SetStatus(string message, bool isError = false)
        {
            state.SetStatus(message, isError);
        }

        public T GetState<T>(string key, T fallback = default)
        {
            return state.GetObjectState($"{EntryId}/{key}", fallback);
        }

        public void SetState<T>(string key, T value)
        {
            state.SetObjectState($"{EntryId}/{key}", value);
        }

        public bool Button(string label, params GUILayoutOption[] options)
        {
            return GUILayout.Button(label, options);
        }
    }
}
