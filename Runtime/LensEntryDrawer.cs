using System;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensEntryDrawer
    {
        public void Draw(LensEntry entry, string sectionTitle, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            var entryId = BuildEntryId(sectionTitle, entry);

            if (entry.IsSensitive)
            {
                DrawReadOnly(entry, entryId, state, styles, metrics);
                DrawInfo(entry, entryId, state, styles, metrics);
                return;
            }

            switch (entry.Kind)
            {
                case LensEntryKind.Toggle:
                    DrawToggle(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.Text:
                    DrawText(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.Number:
                    DrawNumber(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.Button:
                    DrawButton(entry, sectionTitle, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.Slider:
                    DrawSlider(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.SingleSelect:
                    DrawSingleSelect(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.MultiSelect:
                    DrawMultiSelect(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.Progress:
                    DrawProgress(entry, entryId, state, styles, metrics);
                    break;
                case LensEntryKind.Custom:
                    LensEntryDrawerRegistry.GetRequired(entry.CustomTypeId).Draw(new LensEntryDrawContext(sectionTitle, entryId, state, metrics), entry);
                    DrawCustomInfoButton(entry, entryId, state, metrics);
                    break;
                default:
                    DrawReadOnly(entry, entryId, state, styles, metrics);
                    break;
            }

            DrawInfo(entry, entryId, state, styles, metrics);
        }

        private static void DrawReadOnly(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);
            GUILayout.Label(entry.DisplayValue, styles.Value);
            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawToggle(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            var current = entry.GetBoolValue();
            var next = GUILayout.Toggle(current, current ? "Enabled" : "Disabled", GUILayout.Width(metrics.ValueFieldWidth), GUILayout.MinHeight(metrics.ControlHeight));

            if (next != current)
            {
                entry.SetBoolValue(next);
            }

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawText(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            var current = entry.GetTextValue();
            var draft = state.GetTextDraft(entryId, current);
            var next = GUILayout.TextField(draft);

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetTextDraft(entryId, next);
            }

            if (!string.Equals(state.GetTextDraft(entryId, current), current, StringComparison.Ordinal) && GUILayout.Button("Apply", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                entry.SetTextValue(state.GetTextDraft(entryId, current));
            }

            if (!string.Equals(state.GetTextDraft(entryId, current), current, StringComparison.Ordinal) && GUILayout.Button("Revert", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.ResetTextDraft(entryId, current);
            }

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawNumber(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            var draft = state.GetNumberDraft(entryId, entry.GetNumberValue());
            var next = GUILayout.TextField(draft, GUILayout.Width(metrics.ValueFieldWidth), GUILayout.MinHeight(metrics.ControlHeight));

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetNumberDraft(entryId, next);
            }

            if (float.TryParse(state.GetNumberDraft(entryId, entry.GetNumberValue()), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) &&
                Math.Abs(parsed - entry.GetNumberValue()) > 0.0001f &&
                GUILayout.Button("Apply", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                entry.SetNumberValue(parsed);
            }

            if (GUILayout.Button("Revert", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.SetNumberDraft(entryId, entry.GetNumberValue().ToString("0.###", CultureInfo.InvariantCulture));
            }

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawButton(LensEntry entry, string sectionTitle, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            var label = string.IsNullOrWhiteSpace(entry.ActionLabel) ? entry.Key : entry.ActionLabel;
            var actionKey = $"{sectionTitle}/{entry.Key}/{label}";

            if (entry.RequiresConfirmation && state.IsActionConfirmationPending(actionKey))
            {
                var message = string.IsNullOrWhiteSpace(entry.ConfirmationMessage) ? "Confirm action?" : entry.ConfirmationMessage;
                GUILayout.Label(message, styles.Value, GUILayout.MinWidth(160f));

                if (GUILayout.Button("Confirm", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
                {
                    ExecuteButtonAction(entry, state, label);
                    state.ClearActionConfirmation();
                }

                if (GUILayout.Button("Cancel", GUILayout.Width(metrics.SmallButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
                {
                    state.ClearActionConfirmation();
                    state.SetStatus($"{label} cancelled.");
                }

                DrawInfoButton(entry, entryId, state, metrics);
                EndEntry(metrics);
                return;
            }

            if (GUILayout.Button(label, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.MinHeight(metrics.ControlHeight)))
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

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void ExecuteButtonAction(LensEntry entry, LensConsoleState state, string label)
        {
            entry.ExecuteAction();
            state.SetStatus($"{label} executed.");
        }

        private static void DrawSlider(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            var current = entry.GetSliderValue();
            var draft = state.GetSliderDraft(entryId, current);
            var next = GUILayout.HorizontalSlider(draft, entry.GetSliderMin(), entry.GetSliderMax(), GUILayout.Width(metrics.SliderWidth), GUILayout.MinHeight(metrics.ControlHeight));
            next = entry.ClampSliderValue(next);

            if (Math.Abs(next - draft) > 0.0001f)
            {
                state.SetSliderDraft(entryId, next);
            }

            GUILayout.Label(entry.FormatSliderValue(state.GetSliderDraft(entryId, current)), styles.Value, GUILayout.Width(72f));

            if (Math.Abs(state.GetSliderDraft(entryId, current) - current) > 0.0001f && GUILayout.Button("Apply", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                entry.SetSliderValue(state.GetSliderDraft(entryId, current));
            }

            if (GUILayout.Button("Revert", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.ResetSliderDraft(entryId, current);
            }

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawSingleSelect(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            if (GUILayout.Button(entry.DisplayValue, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.TogglePopup(entryId);
            }

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);

            if (!state.IsPopupExpanded(entryId))
            {
                return;
            }

            GUILayout.BeginVertical();
            for (var i = 0; i < entry.GetOptionCount(); i++)
            {
                var selected = entry.IsOptionSelected(i) ? "* " : string.Empty;
                if (GUILayout.Button($"{selected}{entry.GetOptionLabel(i)}", GUILayout.Width(metrics.OptionPopupWidth), GUILayout.MinHeight(metrics.ControlHeight)))
                {
                    entry.SetSingleOption(i);
                    state.ClosePopup(entryId);
                }
            }

            GUILayout.EndVertical();
        }

        private static void DrawMultiSelect(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            if (GUILayout.Button(entry.DisplayValue, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.TogglePopup(entryId);
                state.ResetMultiSelectDraft(entryId, entry.GetSelectedOptionDraft());
            }

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);

            if (!state.IsPopupExpanded(entryId))
            {
                return;
            }

            var draft = state.GetMultiSelectDraft(entryId, entry.GetSelectedOptionDraft());

            GUILayout.BeginVertical();
            if (GUILayout.Button("None", GUILayout.Width(metrics.OptionPopupWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                for (var i = 0; i < draft.Length; i++)
                {
                    draft[i] = false;
                }
            }

            if (GUILayout.Button("Everything", GUILayout.Width(metrics.OptionPopupWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                for (var i = 0; i < draft.Length; i++)
                {
                    draft[i] = true;
                }
            }

            for (var i = 0; i < entry.GetOptionCount(); i++)
            {
                draft[i] = GUILayout.Toggle(draft[i], entry.GetOptionLabel(i), GUILayout.Width(metrics.OptionPopupWidth), GUILayout.MinHeight(metrics.ControlHeight));
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                entry.SetMultiOptions(draft);
                state.ClosePopup(entryId);
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(metrics.SmallButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.ResetMultiSelectDraft(entryId, entry.GetSelectedOptionDraft());
                state.ClosePopup(entryId);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static void DrawProgress(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, styles, metrics);

            var rect = GUILayoutUtility.GetRect(metrics.IsCompact ? metrics.OptionPopupWidth : 160f, metrics.ControlHeight, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.MinHeight(metrics.ControlHeight));
            GUI.Box(rect, GUIContent.none);
            var fill = rect;
            fill.width *= entry.GetProgressRatio();
            GUI.Box(fill, GUIContent.none);
            GUI.Label(rect, entry.DisplayValue, styles.Value);

            DrawInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawInfoButton(LensEntry entry, string entryId, LensConsoleState state, LensLayoutMetrics metrics)
        {
            if (entry.HasInfo && GUILayout.Button("i", GUILayout.Width(metrics.InfoButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.ToggleInfo(entryId);
            }
        }

        private static void DrawCustomInfoButton(LensEntry entry, string entryId, LensConsoleState state, LensLayoutMetrics metrics)
        {
            if (!entry.HasInfo)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            if (!metrics.IsCompact)
            {
                GUILayout.Space(metrics.KeyWidth);
            }

            DrawInfoButton(entry, entryId, state, metrics);
            GUILayout.EndHorizontal();
        }

        private static void DrawInfo(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            if (!entry.HasInfo || !state.IsInfoExpanded(entryId))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            if (!metrics.IsCompact)
            {
                GUILayout.Space(metrics.KeyWidth);
            }

            GUILayout.Label(entry.InfoText, styles.Status);
            GUILayout.EndHorizontal();
        }

        private static void BeginEntry(LensEntry entry, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            if (metrics.IsCompact)
            {
                GUILayout.BeginVertical();
                GUILayout.Label(entry.Key, styles.Key);
                GUILayout.BeginHorizontal();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(metrics.KeyWidth));
        }

        private static void EndEntry(LensLayoutMetrics metrics)
        {
            GUILayout.EndHorizontal();

            if (metrics.IsCompact)
            {
                GUILayout.EndVertical();
            }
        }

        private static string BuildEntryId(string sectionTitle, LensEntry entry)
        {
            return $"{sectionTitle}/{entry.Kind}/{entry.Key}/{entry.CustomTypeId}";
        }
    }
}
