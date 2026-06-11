using System;
using System.Collections.Generic;
using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class LensRuntimeConsole : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private bool showFloatingButton = true;
        [SerializeField] private string floatingButtonLabel = "Lens";
        [SerializeField] private LensUiScaleMode uiScaleMode = LensUiScaleMode.Auto;
        [SerializeField] private float fixedUiScale = 1f;
        [SerializeField] private float minAutoScale = 1f;
        [SerializeField] private float maxAutoScale = 3f;
        [SerializeField] private float refreshIntervalSeconds = 0.25f;

        private readonly LensConsoleState state = new LensConsoleState();
        private readonly LensEntryFilter filter = new LensEntryFilter();
        private readonly LensEntryDrawer entryDrawer = new LensEntryDrawer();
        private readonly LensGuiStyles styles = new LensGuiStyles();
        private readonly LensSectionCache sectionCache = new LensSectionCache();

        internal bool IsOpen => state.IsOpen;

        internal LensRuntimeDiagnostics Diagnostics => sectionCache.Diagnostics;

        public LensUiScaleMode UiScaleMode
        {
            get => uiScaleMode;
            set => uiScaleMode = value;
        }

        public float FixedUiScale
        {
            get => fixedUiScale;
            set => fixedUiScale = Mathf.Clamp(value, 0.5f, 4f);
        }

        public float RefreshIntervalSeconds
        {
            get => refreshIntervalSeconds;
            set => refreshIntervalSeconds = Mathf.Max(0f, value);
        }

        public void SetAutoScaleLimits(float minScale, float maxScale)
        {
            minAutoScale = Mathf.Max(0.5f, minScale);
            maxAutoScale = Mathf.Max(minAutoScale, maxScale);
        }

        public void RefreshNow()
        {
            sectionCache.RequestRefresh();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Open()
        {
            if (!LensRuntimePolicy.IsAllowed)
            {
                return;
            }

            state.Open();
            RefreshNow();
        }

        public void Close()
        {
            state.Close();
        }

        public void Toggle()
        {
            if (!LensRuntimePolicy.IsAllowed)
            {
                return;
            }

            var wasOpen = state.IsOpen;
            state.Toggle();

            if (!wasOpen && state.IsOpen)
            {
                RefreshNow();
            }
        }

        private void OnGUI()
        {
            if (!LensRuntimePolicy.IsAllowed)
            {
                state.Close();
                return;
            }

            styles.EnsureInitialized();
            HandleToggleEvent(Event.current);
            var metrics = LensLayoutMetrics.FromScreen(Screen.width, Screen.height, Screen.dpi, uiScaleMode, fixedUiScale, minAutoScale, maxAutoScale);
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(metrics.UiScale, metrics.UiScale, 1f));

            try
            {
                if (!state.IsOpen)
                {
                    DrawFloatingButton(metrics);
                    return;
                }

                DrawPanel(metrics);
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void HandleToggleEvent(Event currentEvent)
        {
            if (currentEvent == null || currentEvent.type != EventType.KeyDown || currentEvent.keyCode != toggleKey)
            {
                return;
            }

            Toggle();
            currentEvent.Use();
        }

        private void DrawPanel(LensLayoutMetrics metrics)
        {
            var rect = metrics.PanelRect;
            sectionCache.RefreshIfNeeded(LensSectionRegistry.Providers, state, filter, state.SearchText, Time.realtimeSinceStartup, refreshIntervalSeconds);

            GUILayout.BeginArea(rect, GUIContent.none, styles.Panel);
            GUILayout.BeginVertical();

            DrawHeader(metrics);
            DrawSearch(metrics);

            state.ScrollPosition = GUILayout.BeginScrollView(state.ScrollPosition, GUILayout.ExpandHeight(true));

            foreach (var section in sectionCache.Sections)
            {
                DrawProvider(section, metrics);
            }

            GUILayout.EndScrollView();
            DrawFooter(metrics);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawHeader(LensLayoutMetrics metrics)
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

        private void DrawSearch(LensLayoutMetrics metrics)
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
                RefreshNow();
            }

            if (!string.IsNullOrEmpty(state.SearchText) && GUILayout.Button("Clear", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.SearchText = string.Empty;
                RefreshNow();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private void DrawProvider(LensCachedSection section, LensLayoutMetrics metrics)
        {
            if (section == null)
            {
                return;
            }

            var sectionTitle = section.Title;
            var expanded = state.HasSearch || state.IsSectionExpanded(sectionTitle);

            if (GUILayout.Button(section.GetHeaderLabel(expanded), styles.SectionHeader, GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.ToggleSection(sectionTitle);
                RefreshNow();
            }

            if (!expanded)
            {
                GUILayout.Space(4f);
                return;
            }

            foreach (var entry in section.VisibleEntries)
            {
                entryDrawer.Draw(entry, sectionTitle, state, styles, metrics);
            }

            GUILayout.Space(10f);
        }

        private void DrawFooter(LensLayoutMetrics metrics)
        {
            GUILayout.Space(6f);

            if (metrics.IsCompact)
            {
                DrawCompactReportButtons(metrics);

                GUILayout.BeginHorizontal();
                if (state.HasStatus)
                {
                    GUILayout.Label(state.StatusMessage, state.StatusIsError ? styles.ErrorStatus : styles.Status);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label($"Toggle: {toggleKey}", styles.Status);
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.BeginHorizontal();

            DrawReportButton("Copy Text", CopyTextReport, metrics.PrimaryButtonWidth, metrics);
            DrawReportButton("Copy JSON", CopyJsonReport, metrics.PrimaryButtonWidth, metrics);
            DrawReportButton("Screenshot", CaptureScreenshot, metrics.PrimaryButtonWidth, metrics);

            if (state.HasStatus)
            {
                GUILayout.Label(state.StatusMessage, state.StatusIsError ? styles.ErrorStatus : styles.Status);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Toggle: {toggleKey}", styles.Status);
            GUILayout.EndHorizontal();
        }

        private void DrawCompactReportButtons(LensLayoutMetrics metrics)
        {
            GUILayout.BeginHorizontal();
            DrawReportButton("Copy Text", CopyTextReport, metrics.PrimaryButtonWidth, metrics);
            DrawReportButton("Copy JSON", CopyJsonReport, metrics.PrimaryButtonWidth, metrics);
            GUILayout.EndHorizontal();

            DrawReportButton("Capture Screenshot", CaptureScreenshot, metrics.PrimaryButtonWidth * 2f, metrics);
        }

        private void DrawReportButton(string label, Action action, float width, LensLayoutMetrics metrics)
        {
            if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                action();
            }
        }

        private void CopyTextReport()
        {
            ForceRefreshAllSections();
            GUIUtility.systemCopyBuffer = LensReportBuilder.BuildTextReport(LensSectionRegistry.Providers);
            state.SetStatus("Text report copied.");
        }

        private void CopyJsonReport()
        {
            ForceRefreshAllSections();
            GUIUtility.systemCopyBuffer = LensReportBuilder.BuildJsonReport(LensSectionRegistry.Providers);
            state.SetStatus("JSON report copied.");
        }

        private void CaptureScreenshot()
        {
            ForceRefreshAllSections();
            var screenshot = LensReportCapture.CaptureScreenshot();
            GUIUtility.systemCopyBuffer = screenshot.Path;
            state.SetStatus($"Screenshot path copied: {screenshot.Path}");
        }

        private void ForceRefreshAllSections()
        {
            sectionCache.ForceRefresh(LensSectionRegistry.Providers, state, filter, state.SearchText, Time.realtimeSinceStartup, true);
        }

        private void DrawFloatingButton(LensLayoutMetrics metrics)
        {
            if (!showFloatingButton)
            {
                return;
            }

            state.FloatingButtonRect = metrics.ClampFloatingButton(state.FloatingButtonRect);
            state.FloatingButtonRect = GUI.Window(GetInstanceID(), state.FloatingButtonRect, DrawFloatingButtonWindow, GUIContent.none, styles.FloatingButtonWindow);
            state.FloatingButtonRect = metrics.ClampFloatingButton(state.FloatingButtonRect);
        }

        private void DrawFloatingButtonWindow(int windowId)
        {
            if (GUILayout.Button(string.IsNullOrWhiteSpace(floatingButtonLabel) ? "Lens" : floatingButtonLabel))
            {
                Open();
            }

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }
    }
}
