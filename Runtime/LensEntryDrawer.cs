using System;
using System.Globalization;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensEntryDrawer
    {
        public void Draw(LensEntry entry, string sectionTitle, string sectionIdentity, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            var entryId = state.GetEntryId(sectionIdentity, entry);

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
                    DrawButton(entry, sectionIdentity, entryId, state, styles, metrics);
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
            BeginEntry(entry, entryId, state, styles, metrics);
            GUILayout.Label(entry.DisplayValue, styles.Value);
            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawToggle(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            var current = entry.GetBoolValue();
            var next = GUILayout.Toggle(current, current ? "Enabled" : "Disabled", GUILayout.Width(metrics.ValueFieldWidth), GUILayout.Height(metrics.ControlHeight));

            if (next != current)
            {
                entry.SetBoolValue(next);
            }

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawText(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            var current = entry.GetTextValue();
            var draft = state.GetTextDraft(entryId, current);
            var next = GUILayout.TextField(draft);

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetTextDraft(entryId, next);
                draft = next;
            }

            var hasChanges = !string.Equals(draft, current, StringComparison.Ordinal);
            if (hasChanges && GUILayout.Button("Apply", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                entry.SetTextValue(draft);
            }

            if (hasChanges && GUILayout.Button("Revert", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                state.ResetTextDraft(entryId, current);
            }

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawNumber(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            var current = entry.GetNumberValue();
            var draft = state.GetNumberDraft(entryId, current);
            var next = GUILayout.TextField(draft, GUILayout.Width(metrics.ValueFieldWidth), GUILayout.Height(metrics.ControlHeight));

            if (!string.Equals(next, draft, StringComparison.Ordinal))
            {
                state.SetNumberDraft(entryId, next);
                draft = next;
            }

            if (float.TryParse(draft, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) &&
                Math.Abs(parsed - current) > 0.0001f &&
                GUILayout.Button("Apply", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                entry.SetNumberValue(parsed);
            }

            if (GUILayout.Button("Revert", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                state.SetNumberDraft(entryId, current.ToString("0.###", CultureInfo.InvariantCulture));
            }

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawButton(LensEntry entry, string sectionIdentity, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            var label = string.IsNullOrWhiteSpace(entry.ActionLabel) ? entry.Key : entry.ActionLabel;
            var actionKey = $"{sectionIdentity}/{entry.Key}/{label}";

            if (entry.RequiresConfirmation && state.IsActionConfirmationPending(actionKey))
            {
                var message = string.IsNullOrWhiteSpace(entry.ConfirmationMessage) ? "Confirm action?" : entry.ConfirmationMessage;
                GUILayout.Label(message, styles.Value, GUILayout.MinWidth(160f));

                if (GUILayout.Button("Confirm", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.Height(metrics.ControlHeight)))
                {
                    ExecuteButtonAction(entry, state, label);
                    state.ClearActionConfirmation();
                }

                if (GUILayout.Button("Cancel", GUILayout.Width(metrics.SmallButtonWidth), GUILayout.Height(metrics.ControlHeight)))
                {
                    state.ClearActionConfirmation();
                    state.SetStatus($"{label} cancelled.");
                }

                DrawValueRowInfoButton(entry, entryId, state, metrics);
                EndEntry(metrics);
                return;
            }

            if (GUILayout.Button(label, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.Height(metrics.ControlHeight)))
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

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void ExecuteButtonAction(LensEntry entry, LensConsoleState state, string label)
        {
            entry.ExecuteAction();
            state.SetStatus($"{label} executed.");
        }

        private static void DrawSlider(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            var current = entry.GetSliderValue();
            var draft = state.GetSliderDraft(entryId, current);
            var next = GUILayout.HorizontalSlider(draft, entry.GetSliderMin(), entry.GetSliderMax(), GUILayout.Width(metrics.SliderWidth), GUILayout.Height(metrics.ControlHeight));
            next = entry.ClampSliderValue(next);

            if (Math.Abs(next - draft) > 0.0001f)
            {
                state.SetSliderDraft(entryId, next);
                draft = next;
            }

            GUILayout.Label(entry.FormatSliderValue(draft), styles.Value, GUILayout.Width(72f));

            if (Math.Abs(draft - current) > 0.0001f && GUILayout.Button("Apply", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                entry.SetSliderValue(draft);
            }

            if (GUILayout.Button("Revert", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                state.ResetSliderDraft(entryId, current);
            }

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawSingleSelect(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            if (GUILayout.Button(entry.DisplayValue, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.Height(metrics.ControlHeight)))
            {
                state.TogglePopup(entryId);
            }

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);

            if (!state.IsPopupExpanded(entryId))
            {
                return;
            }

            GUILayout.BeginVertical();
            var optionCount = entry.GetOptionCount();
            for (var i = 0; i < optionCount; i++)
            {
                var selected = entry.IsOptionSelected(i) ? "* " : string.Empty;
                if (GUILayout.Button($"{selected}{entry.GetOptionLabel(i)}", GUILayout.Width(metrics.OptionPopupWidth), GUILayout.Height(metrics.ControlHeight)))
                {
                    entry.SetSingleOption(i);
                    state.ClosePopup(entryId);
                }
            }

            GUILayout.EndVertical();
        }

        private static void DrawMultiSelect(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            if (GUILayout.Button(entry.DisplayValue, GUILayout.Width(metrics.IsCompact ? metrics.OptionPopupWidth : 180f), GUILayout.Height(metrics.ControlHeight)))
            {
                state.TogglePopup(entryId);
                var selected = entry.GetSelectedOptionDraft();
                state.ResetMultiSelectDraft(entryId, selected);
            }

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);

            if (!state.IsPopupExpanded(entryId))
            {
                return;
            }

            var currentSelection = entry.GetSelectedOptionDraft();
            var draft = state.GetMultiSelectDraft(entryId, currentSelection);

            GUILayout.BeginVertical();
            if (GUILayout.Button("None", GUILayout.Width(metrics.OptionPopupWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                for (var i = 0; i < draft.Length; i++)
                {
                    draft[i] = false;
                }
            }

            if (GUILayout.Button("Everything", GUILayout.Width(metrics.OptionPopupWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                for (var i = 0; i < draft.Length; i++)
                {
                    draft[i] = true;
                }
            }

            var optionCount = entry.GetOptionCount();
            for (var i = 0; i < optionCount; i++)
            {
                draft[i] = GUILayout.Toggle(draft[i], entry.GetOptionLabel(i), GUILayout.Width(metrics.OptionPopupWidth), GUILayout.Height(metrics.ControlHeight));
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Width(metrics.RevertButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                entry.SetMultiOptions(draft);
                state.ClosePopup(entryId);
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(metrics.SmallButtonWidth), GUILayout.Height(metrics.ControlHeight)))
            {
                state.ResetMultiSelectDraft(entryId, currentSelection);
                state.ClosePopup(entryId);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static void DrawProgress(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            BeginEntry(entry, entryId, state, styles, metrics);

            if (metrics.IsCompact)
            {
                GUILayout.BeginVertical();
                GUILayout.Label(entry.DisplayValue, styles.Value);
                var compactRect = GUILayoutUtility.GetRect(
                    metrics.OptionPopupWidth,
                    metrics.ProgressBarHeight,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(metrics.ProgressBarHeight));
                DrawProgressBar(entry, styles, compactRect, false);
                GUILayout.EndVertical();
                EndEntry(metrics);
                return;
            }

            var rect = GUILayoutUtility.GetRect(
                160f,
                metrics.ProgressBarHeight,
                GUILayout.Width(180f),
                GUILayout.Height(metrics.ProgressBarHeight));
            DrawProgressBar(entry, styles, rect, true);

            DrawValueRowInfoButton(entry, entryId, state, metrics);
            EndEntry(metrics);
        }

        private static void DrawProgressBar(LensEntry entry, LensGuiStyles styles, Rect rect, bool drawLabel)
        {
            var ratio = entry.GetProgressRatio();
            GUI.Box(rect, GUIContent.none);
            var fill = rect;
            fill.width *= ratio;
            GUI.Box(fill, GUIContent.none);

            if (drawLabel)
            {
                GUI.Label(rect, entry.DisplayValue, styles.Value);
            }
        }

        private static void DrawValueRowInfoButton(LensEntry entry, string entryId, LensConsoleState state, LensLayoutMetrics metrics)
        {
            if (!metrics.IsCompact)
            {
                DrawInfoButton(entry, entryId, state, metrics);
            }
        }

        private static void DrawInfoButton(LensEntry entry, string entryId, LensConsoleState state, LensLayoutMetrics metrics)
        {
            if (entry.HasInfo && GUILayout.Button("i", GUILayout.Width(metrics.InfoButtonWidth), GUILayout.Height(metrics.InfoButtonHeight)))
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

        private static void BeginEntry(LensEntry entry, string entryId, LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            if (metrics.IsCompact)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Key, styles.Key);
                GUILayout.FlexibleSpace();
                DrawInfoButton(entry, entryId, state, metrics);
                GUILayout.EndHorizontal();
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
    }
}
