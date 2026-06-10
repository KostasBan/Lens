using System;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensEntryDrawer
    {
        public void Draw(LensEntry entry, string sectionTitle, LensConsoleState state, LensGuiStyles styles)
        {
            if (entry.IsSensitive)
            {
                DrawReadOnly(entry, styles);
                return;
            }

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
                    DrawButton(entry, sectionTitle, state, styles);
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
            GUILayout.Label(entry.DisplayValue, styles.Value);
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

        private static void DrawButton(LensEntry entry, string sectionTitle, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var label = string.IsNullOrWhiteSpace(entry.ActionLabel) ? entry.Key : entry.ActionLabel;
            var actionKey = $"{sectionTitle}/{entry.Key}/{label}";

            if (entry.RequiresConfirmation && state.IsActionConfirmationPending(actionKey))
            {
                var message = string.IsNullOrWhiteSpace(entry.ConfirmationMessage) ? "Confirm action?" : entry.ConfirmationMessage;
                GUILayout.Label(message, styles.Value, GUILayout.MinWidth(160f));

                if (GUILayout.Button("Confirm", GUILayout.Width(82f)))
                {
                    ExecuteButtonAction(entry, state, label);
                    state.ClearActionConfirmation();
                }

                if (GUILayout.Button("Cancel", GUILayout.Width(72f)))
                {
                    state.ClearActionConfirmation();
                    state.SetStatus($"{label} cancelled.");
                }

                GUILayout.EndHorizontal();
                return;
            }

            if (GUILayout.Button(label, GUILayout.Width(180f)))
            {
                if (entry.RequiresConfirmation)
                {
                    state.RequestActionConfirmation(actionKey);
                    state.SetStatus($"{label} needs confirmation.");
                }
                else
                {
                    ExecuteButtonAction(entry, state, label);
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void ExecuteButtonAction(LensEntry entry, LensConsoleState state, string label)
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
    }
}
