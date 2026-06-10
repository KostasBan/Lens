using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensConsoleState
    {
        private const float StatusDuration = 2f;

        private readonly Dictionary<string, bool> expandedSections = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> numberDrafts = new Dictionary<string, string>();

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
    }
}
