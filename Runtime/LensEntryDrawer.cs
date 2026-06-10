using System;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensEntryDrawer
    {
        public void Draw(LensEntry entry, string sectionTitle, LensConsoleState state, LensGuiStyles styles)
        {
            var entryId = BuildEntryId(sectionTitle, entry);

            if (entry.IsSensitive)
            {
                DrawReadOnly(entry, entryId, state, styles);
                DrawInfo(entry, entryId, state, styles);
                return;
            }

            switch (entry.Kind)
            {
                case LensEntryKind.Toggle:
                    DrawToggle(entry, entryId, state, styles);
                    break;
                case LensEntryKind.Text:
                    DrawText(entry, entryId, state, styles);
                    break;
                case LensEntryKind.Number:
                    DrawNumber(entry, entryId, state, styles);
                    break;
                case LensEntryKind.Button:
                    DrawButton(entry, sectionTitle, entryId, state, styles);
                    break;
                case LensEntryKind.Slider:
                    DrawSlider(entry, entryId, state, styles);
                    break;
                case LensEntryKind.SingleSelect:
                    DrawSingleSelect(entry, entryId, state, styles);
                    break;
                case LensEntryKind.MultiSelect:
                    DrawMultiSelect(entry, entryId, state, styles);
                    break;
                case LensEntryKind.Progress:
                    DrawProgress(entry, entryId, state, styles);
                    break;
                case LensEntryKind.Custom:
                    LensEntryDrawerRegistry.GetRequired(entry.CustomTypeId).Draw(new LensEntryDrawContext(sectionTitle, entryId, state), entry);
                    DrawCustomInfoButton(entry, entryId, state);
                    break;
                default:
                    DrawReadOnly(entry, entryId, state, styles);
                    break;
            }

            DrawInfo(entry, entryId, state, styles);
        }

        private static void DrawReadOnly(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));
            GUILayout.Label(entry.DisplayValue, styles.Value);
            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawToggle(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var current = entry.GetBoolValue();
            var next = GUILayout.Toggle(current, current ? "Enabled" : "Disabled", GUILayout.Width(120f));

            if (next != current)
            {
                entry.SetBoolValue(next);
            }

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawText(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var current = entry.GetTextValue();
            var draft = state.GetTextDraft(entryId, current);
            var next = GUILayout.TextField(draft);

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetTextDraft(entryId, next);
            }

            if (!string.Equals(state.GetTextDraft(entryId, current), current, StringComparison.Ordinal) && GUILayout.Button("Apply", GUILayout.Width(58f)))
            {
                entry.SetTextValue(state.GetTextDraft(entryId, current));
            }

            if (!string.Equals(state.GetTextDraft(entryId, current), current, StringComparison.Ordinal) && GUILayout.Button("Revert", GUILayout.Width(62f)))
            {
                state.ResetTextDraft(entryId, current);
            }

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawNumber(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var draft = state.GetNumberDraft(entryId, entry.GetNumberValue());
            var next = GUILayout.TextField(draft, GUILayout.Width(120f));

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetNumberDraft(entryId, next);
            }

            if (float.TryParse(state.GetNumberDraft(entryId, entry.GetNumberValue()), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) &&
                Math.Abs(parsed - entry.GetNumberValue()) > 0.0001f &&
                GUILayout.Button("Apply", GUILayout.Width(58f)))
            {
                entry.SetNumberValue(parsed);
            }

            if (GUILayout.Button("Revert", GUILayout.Width(62f)))
            {
                state.SetNumberDraft(entryId, entry.GetNumberValue().ToString("0.###", CultureInfo.InvariantCulture));
            }

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawButton(LensEntry entry, string sectionTitle, string entryId, LensConsoleState state, LensGuiStyles styles)
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

                DrawInfoButton(entry, entryId, state);
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

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void ExecuteButtonAction(LensEntry entry, LensConsoleState state, string label)
        {
            entry.ExecuteAction();
            state.SetStatus($"{label} executed.");
        }

        private static void DrawSlider(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var current = entry.GetSliderValue();
            var draft = state.GetSliderDraft(entryId, current);
            var next = GUILayout.HorizontalSlider(draft, entry.GetSliderMin(), entry.GetSliderMax(), GUILayout.Width(140f));
            next = entry.ClampSliderValue(next);

            if (Math.Abs(next - draft) > 0.0001f)
            {
                state.SetSliderDraft(entryId, next);
            }

            GUILayout.Label(entry.FormatSliderValue(state.GetSliderDraft(entryId, current)), styles.Value, GUILayout.Width(72f));

            if (Math.Abs(state.GetSliderDraft(entryId, current) - current) > 0.0001f && GUILayout.Button("Apply", GUILayout.Width(58f)))
            {
                entry.SetSliderValue(state.GetSliderDraft(entryId, current));
            }

            if (GUILayout.Button("Revert", GUILayout.Width(62f)))
            {
                state.ResetSliderDraft(entryId, current);
            }

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawSingleSelect(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            if (GUILayout.Button(entry.DisplayValue, GUILayout.Width(180f)))
            {
                state.TogglePopup(entryId);
            }

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();

            if (!state.IsPopupExpanded(entryId))
            {
                return;
            }

            GUILayout.BeginVertical();
            for (var i = 0; i < entry.GetOptionCount(); i++)
            {
                var selected = entry.IsOptionSelected(i) ? "* " : string.Empty;
                if (GUILayout.Button($"{selected}{entry.GetOptionLabel(i)}", GUILayout.Width(240f)))
                {
                    entry.SetSingleOption(i);
                    state.ClosePopup(entryId);
                }
            }

            GUILayout.EndVertical();
        }

        private static void DrawMultiSelect(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            if (GUILayout.Button(entry.DisplayValue, GUILayout.Width(180f)))
            {
                state.TogglePopup(entryId);
                state.ResetMultiSelectDraft(entryId, entry.GetSelectedOptionDraft());
            }

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();

            if (!state.IsPopupExpanded(entryId))
            {
                return;
            }

            var draft = state.GetMultiSelectDraft(entryId, entry.GetSelectedOptionDraft());

            GUILayout.BeginVertical();
            if (GUILayout.Button("None", GUILayout.Width(240f)))
            {
                for (var i = 0; i < draft.Length; i++)
                {
                    draft[i] = false;
                }
            }

            if (GUILayout.Button("Everything", GUILayout.Width(240f)))
            {
                for (var i = 0; i < draft.Length; i++)
                {
                    draft[i] = true;
                }
            }

            for (var i = 0; i < entry.GetOptionCount(); i++)
            {
                draft[i] = GUILayout.Toggle(draft[i], entry.GetOptionLabel(i), GUILayout.Width(240f));
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Width(82f)))
            {
                entry.SetMultiOptions(draft);
                state.ClosePopup(entryId);
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(72f)))
            {
                state.ResetMultiSelectDraft(entryId, entry.GetSelectedOptionDraft());
                state.ClosePopup(entryId);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static void DrawProgress(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));

            var rect = GUILayoutUtility.GetRect(160f, 18f, GUILayout.Width(180f));
            GUI.Box(rect, GUIContent.none);
            var fill = rect;
            fill.width *= entry.GetProgressRatio();
            GUI.Box(fill, GUIContent.none);
            GUI.Label(rect, entry.DisplayValue, styles.Value);

            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawInfoButton(LensEntry entry, string entryId, LensConsoleState state)
        {
            if (entry.HasInfo && GUILayout.Button("i", GUILayout.Width(24f)))
            {
                state.ToggleInfo(entryId);
            }
        }

        private static void DrawCustomInfoButton(LensEntry entry, string entryId, LensConsoleState state)
        {
            if (!entry.HasInfo)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(190f);
            DrawInfoButton(entry, entryId, state);
            GUILayout.EndHorizontal();
        }

        private static void DrawInfo(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles)
        {
            if (!entry.HasInfo || !state.IsInfoExpanded(entryId))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(190f);
            GUILayout.Label(entry.InfoText, styles.Status);
            GUILayout.EndHorizontal();
        }

        private static string BuildEntryId(string sectionTitle, LensEntry entry)
        {
            return $"{sectionTitle}/{entry.Kind}/{entry.Key}/{entry.CustomTypeId}";
        }
    }
}
