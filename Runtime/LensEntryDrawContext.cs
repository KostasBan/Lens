using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class LensEntryDrawContext
    {
        private readonly LensConsoleState state;

        internal LensEntryDrawContext(string sectionTitle, string entryId, LensConsoleState state)
        {
            SectionTitle = sectionTitle ?? string.Empty;
            EntryId = entryId ?? string.Empty;
            this.state = state;
        }

        public string SectionTitle { get; }

        public string EntryId { get; }

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
