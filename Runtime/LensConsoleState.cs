using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensConsoleState
    {
        private const float StatusDuration = 2f;

        private readonly Dictionary<string, bool> expandedSections = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> expandedInfo = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> expandedPopups = new Dictionary<string, bool>();
        private readonly Dictionary<string, object> objectStates = new Dictionary<string, object>();
        private readonly Dictionary<string, string> textDrafts = new Dictionary<string, string>();
        private readonly Dictionary<string, string> numberDrafts = new Dictionary<string, string>();
        private readonly Dictionary<string, float> sliderDrafts = new Dictionary<string, float>();
        private readonly Dictionary<string, bool[]> multiSelectDrafts = new Dictionary<string, bool[]>();
        private readonly Dictionary<LensEntryIdKey, string> entryIds = new Dictionary<LensEntryIdKey, string>();

        private string pendingActionKey = string.Empty;

        public bool IsOpen { get; private set; }

        public string SearchText { get; set; } = string.Empty;

        public Vector2 ScrollPosition { get; set; }

        public Rect FloatingButtonRect { get; set; } = new Rect(18f, 120f, 72f, 36f);

        public string StatusMessage { get; private set; } = string.Empty;

        public bool StatusIsError { get; private set; }

        private float statusUntil = -100f;

        public bool HasSearch => !string.IsNullOrWhiteSpace(SearchText);

        public bool HasStatus => Time.realtimeSinceStartup <= statusUntil && !string.IsNullOrEmpty(StatusMessage);

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public bool IsActionConfirmationPending(string actionKey)
        {
            return !string.IsNullOrEmpty(actionKey) && string.Equals(pendingActionKey, actionKey, System.StringComparison.Ordinal);
        }

        public void RequestActionConfirmation(string actionKey)
        {
            pendingActionKey = actionKey ?? string.Empty;
        }

        public void ClearActionConfirmation()
        {
            pendingActionKey = string.Empty;
        }

        public bool IsInfoExpanded(string entryId)
        {
            return expandedInfo.TryGetValue(entryId ?? string.Empty, out var expanded) && expanded;
        }

        public void ToggleInfo(string entryId)
        {
            var key = entryId ?? string.Empty;
            expandedInfo[key] = !IsInfoExpanded(key);
        }

        public bool IsPopupExpanded(string entryId)
        {
            return expandedPopups.TryGetValue(entryId ?? string.Empty, out var expanded) && expanded;
        }

        public void TogglePopup(string entryId)
        {
            var key = entryId ?? string.Empty;
            expandedPopups[key] = !IsPopupExpanded(key);
        }

        public void ClosePopup(string entryId)
        {
            expandedPopups[entryId ?? string.Empty] = false;
        }

        public bool IsSectionExpanded(string sectionTitle)
        {
            return !expandedSections.TryGetValue(sectionTitle ?? string.Empty, out var expanded) || expanded;
        }

        public void ToggleSection(string sectionTitle)
        {
            var key = sectionTitle ?? string.Empty;
            expandedSections[key] = !IsSectionExpanded(key);
        }

        public void SetStatus(string message, bool isError = false)
        {
            StatusMessage = message ?? string.Empty;
            StatusIsError = isError;
            statusUntil = Time.realtimeSinceStartup + StatusDuration;
        }

        public string GetEntryId(string sectionTitle, LensEntry entry)
        {
            var key = new LensEntryIdKey(sectionTitle, entry.Kind, entry.Key, entry.CustomTypeId);
            if (entryIds.TryGetValue(key, out var entryId))
            {
                return entryId;
            }

            entryId = string.Concat(sectionTitle ?? string.Empty, "/", entry.Kind.ToString(), "/", entry.Key ?? string.Empty, "/", entry.CustomTypeId ?? string.Empty);
            entryIds[key] = entryId;
            return entryId;
        }

        public string GetNumberDraft(string key, float currentValue)
        {
            if (numberDrafts.TryGetValue(key, out var draft))
            {
                return draft;
            }

            draft = currentValue.ToString("0.###", CultureInfo.InvariantCulture);
            numberDrafts[key] = draft;
            return draft;
        }

        public void SetNumberDraft(string key, string value)
        {
            numberDrafts[key] = value ?? string.Empty;
        }

        public string GetTextDraft(string key, string currentValue)
        {
            if (textDrafts.TryGetValue(key, out var draft))
            {
                return draft;
            }

            draft = currentValue ?? string.Empty;
            textDrafts[key] = draft;
            return draft;
        }

        public void SetTextDraft(string key, string value)
        {
            textDrafts[key] = value ?? string.Empty;
        }

        public void ResetTextDraft(string key, string currentValue)
        {
            textDrafts[key] = currentValue ?? string.Empty;
        }

        public float GetSliderDraft(string key, float currentValue)
        {
            if (sliderDrafts.TryGetValue(key, out var draft))
            {
                return draft;
            }

            sliderDrafts[key] = currentValue;
            return currentValue;
        }

        public void SetSliderDraft(string key, float value)
        {
            sliderDrafts[key] = value;
        }

        public void ResetSliderDraft(string key, float currentValue)
        {
            sliderDrafts[key] = currentValue;
        }

        public bool[] GetMultiSelectDraft(string key, bool[] currentValue)
        {
            if (multiSelectDrafts.TryGetValue(key, out var draft) && draft.Length == currentValue.Length)
            {
                return draft;
            }

            draft = (bool[])currentValue.Clone();
            multiSelectDrafts[key] = draft;
            return draft;
        }

        public void SetMultiSelectDraft(string key, bool[] value)
        {
            CopyMultiSelectDraft(key, value);
        }

        public void ResetMultiSelectDraft(string key, bool[] currentValue)
        {
            CopyMultiSelectDraft(key, currentValue);
        }

        private void CopyMultiSelectDraft(string key, bool[] value)
        {
            value = value ?? new bool[0];

            if (!multiSelectDrafts.TryGetValue(key, out var draft) || draft.Length != value.Length)
            {
                multiSelectDrafts[key] = (bool[])value.Clone();
                return;
            }

            for (var i = 0; i < value.Length; i++)
            {
                draft[i] = value[i];
            }
        }

        public T GetObjectState<T>(string key, T fallback)
        {
            return objectStates.TryGetValue(key ?? string.Empty, out var value) && value is T typed ? typed : fallback;
        }

        public void SetObjectState<T>(string key, T value)
        {
            objectStates[key ?? string.Empty] = value;
        }

        private readonly struct LensEntryIdKey : System.IEquatable<LensEntryIdKey>
        {
            private readonly string sectionTitle;
            private readonly LensEntryKind kind;
            private readonly string entryKey;
            private readonly string customTypeId;

            public LensEntryIdKey(string sectionTitle, LensEntryKind kind, string entryKey, string customTypeId)
            {
                this.sectionTitle = sectionTitle ?? string.Empty;
                this.kind = kind;
                this.entryKey = entryKey ?? string.Empty;
                this.customTypeId = customTypeId ?? string.Empty;
            }

            public bool Equals(LensEntryIdKey other)
            {
                return kind == other.kind &&
                       string.Equals(sectionTitle, other.sectionTitle, System.StringComparison.Ordinal) &&
                       string.Equals(entryKey, other.entryKey, System.StringComparison.Ordinal) &&
                       string.Equals(customTypeId, other.customTypeId, System.StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is LensEntryIdKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = sectionTitle.GetHashCode();
                    hash = (hash * 397) ^ (int)kind;
                    hash = (hash * 397) ^ entryKey.GetHashCode();
                    hash = (hash * 397) ^ customTypeId.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
