using System;
using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensConsoleChrome
    {
        public void DrawHeader(LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lens", styles.Title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUILayout.Width(metrics.SmallButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.Close();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        public void DrawSearch(LensConsoleState state, LensGuiStyles styles, LensLayoutMetrics metrics, Action refresh)
        {
            if (metrics.IsCompact)
            {
                GUILayout.Label("Search", styles.Key);
                GUILayout.BeginHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Search", styles.Key, GUILayout.Width(metrics.SearchLabelWidth));
            }

            var nextSearch = GUILayout.TextField(state.SearchText, styles.SearchField, GUILayout.MinHeight(metrics.ControlHeight));

            if (!string.Equals(nextSearch, state.SearchText, StringComparison.Ordinal))
            {
                state.SearchText = nextSearch ?? string.Empty;
                refresh();
            }

            if (!string.IsNullOrEmpty(state.SearchText) && GUILayout.Button("Clear", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.SearchText = string.Empty;
                refresh();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        public void DrawFooter(
            LensConsoleState state,
            LensGuiStyles styles,
            LensLayoutMetrics metrics,
            KeyCode toggleKey,
            Action copyText,
            Action copyJson,
            Action captureScreenshot,
            Action share)
        {
            GUILayout.Space(6f);

            if (metrics.IsCompact)
            {
                GUILayout.BeginHorizontal();
                DrawReportButton("Copy Text", copyText, metrics.PrimaryButtonWidth, metrics);
                DrawReportButton("Copy JSON", copyJson, metrics.PrimaryButtonWidth, metrics);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                DrawReportButton("Screenshot", captureScreenshot, metrics.PrimaryButtonWidth, metrics);
                DrawReportButton("Share", share, metrics.PrimaryButtonWidth, metrics);
                GUILayout.EndHorizontal();

                DrawStatus(state, styles, toggleKey);
                return;
            }

            GUILayout.BeginHorizontal();

            DrawReportButton("Copy Text", copyText, metrics.PrimaryButtonWidth, metrics);
            DrawReportButton("Copy JSON", copyJson, metrics.PrimaryButtonWidth, metrics);
            DrawReportButton("Screenshot", captureScreenshot, metrics.PrimaryButtonWidth, metrics);
            DrawReportButton("Share", share, metrics.PrimaryButtonWidth, metrics);

            if (state.HasStatus)
            {
                GUILayout.Label(state.StatusMessage, state.StatusIsError ? styles.ErrorStatus : styles.Status);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Toggle: {toggleKey}", styles.Status);
            GUILayout.EndHorizontal();
        }

        private static void DrawStatus(LensConsoleState state, LensGuiStyles styles, KeyCode toggleKey)
        {
            GUILayout.BeginHorizontal();
            if (state.HasStatus)
            {
                GUILayout.Label(state.StatusMessage, state.StatusIsError ? styles.ErrorStatus : styles.Status);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Toggle: {toggleKey}", styles.Status);
            GUILayout.EndHorizontal();
        }

        private static void DrawReportButton(string label, Action action, float width, LensLayoutMetrics metrics)
        {
            if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                action();
            }
        }
    }
}
