using System;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensEntryDrawer
    {
        public void Draw(LensEntry entry, string sectionTitle, LensConsoleState state, LensGuiStyles styles)
        {
            switch (entry.Kind)
            {
                case LensEntryKind.Toggle:
                    DrawToggle(entry, styles);
                    break;
                case LensEntryKind.Text:
                    DrawText(entry, styles);
                    break;
                case LensEntryKind.Number:
                    DrawNumber(entry, sectionTitle, state, styles);
                    break;
                case LensEntryKind.Button:
                    DrawButton(entry, state, styles);
                    break;
                default:
                    DrawReadOnly(entry, styles);
                    break;
            }
        }

        private static void DrawReadOnly(LensEntry entry, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));
            GUILayout.Label(entry.Value, styles.Value);
            GUILayout.EndHorizontal();
        }

        private static void DrawToggle(LensEntry entry, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var current = entry.GetBoolValue();
            var next = GUILayout.Toggle(current, current ? "Enabled" : "Disabled", GUILayout.Width(120f));

            if (next != current)
            {
                entry.SetBoolValue(next);
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawText(LensEntry entry, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var current = entry.GetTextValue();
            var next = GUILayout.TextField(current);

            if (!string.Equals(next, current, StringComparison.Ordinal))
            {
                entry.SetTextValue(next);
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawNumber(LensEntry entry, string sectionTitle, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var draftKey = $"{sectionTitle}/{entry.Key}";
            var draft = state.GetNumberDraft(draftKey, entry.GetNumberValue());
            var next = GUILayout.TextField(draft, GUILayout.Width(120f));

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetNumberDraft(draftKey, next);

                if (float.TryParse(next, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    entry.SetNumberValue(parsed);
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawButton(LensEntry entry, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var label = string.IsNullOrWhiteSpace(entry.ActionLabel) ? entry.Key : entry.ActionLabel;

            if (GUILayout.Button(label, GUILayout.Width(180f)))
            {
                try
                {
                    entry.ExecuteAction();
                    state.SetStatus($"{label} executed.");
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    state.SetStatus($"{label} failed.", true);
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}
