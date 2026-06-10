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

        private readonly List<LensEntry> entryBuffer = new List<LensEntry>();
        private readonly List<LensEntry> visibleEntries = new List<LensEntry>();
        private readonly LensConsoleState state = new LensConsoleState();
        private readonly LensEntryFilter filter = new LensEntryFilter();
        private readonly LensEntryDrawer entryDrawer = new LensEntryDrawer();
        private readonly LensGuiStyles styles = new LensGuiStyles();

        internal bool IsOpen => state.IsOpen;

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

        public void SetAutoScaleLimits(float minScale, float maxScale)
        {
            minAutoScale = Mathf.Max(0.5f, minScale);
            maxAutoScale = Mathf.Max(minAutoScale, maxScale);
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

            state.Toggle();
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

            state.Toggle();
            currentEvent.Use();
        }

        private void DrawPanel(LensLayoutMetrics metrics)
        {
            var rect = metrics.PanelRect;

            GUILayout.BeginArea(rect, GUIContent.none, styles.Panel);
            GUILayout.BeginVertical();

            DrawHeader(metrics);
            DrawSearch(metrics);

            state.ScrollPosition = GUILayout.BeginScrollView(state.ScrollPosition, GUILayout.ExpandHeight(true));

            foreach (var provider in LensSectionRegistry.Providers)
            {
                DrawProvider(provider, metrics);
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
            }

            if (!string.IsNullOrEmpty(state.SearchText) && GUILayout.Button("Clear", GUILayout.Width(metrics.ApplyButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.SearchText = string.Empty;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private void DrawProvider(ILensSectionProvider provider, LensLayoutMetrics metrics)
        {
            if (provider == null)
            {
                return;
            }

            var sectionTitle = string.IsNullOrWhiteSpace(provider.SectionTitle) ? "Untitled" : provider.SectionTitle;

            entryBuffer.Clear();
            visibleEntries.Clear();

            foreach (var entry in provider.GetEntries())
            {
                entryBuffer.Add(entry);
            }

            var sectionMatchesSearch = filter.MatchesSection(sectionTitle, state.SearchText);

            foreach (var entry in entryBuffer)
            {
                if (!state.HasSearch || sectionMatchesSearch || filter.MatchesEntry(entry, state.SearchText))
                {
                    visibleEntries.Add(entry);
                }
            }

            if (state.HasSearch && visibleEntries.Count == 0)
            {
                return;
            }

            var expanded = state.HasSearch || state.IsSectionExpanded(sectionTitle);
            var marker = expanded ? "[-]" : "[+]";

            if (GUILayout.Button($"{marker} {sectionTitle} ({visibleEntries.Count})", styles.SectionHeader, GUILayout.MinHeight(metrics.ControlHeight)))
            {
                state.ToggleSection(sectionTitle);
            }

            if (!expanded)
            {
                GUILayout.Space(4f);
                return;
            }

            foreach (var entry in visibleEntries)
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
                if (GUILayout.Button("Copy Debug Report", GUILayout.Width(metrics.PrimaryButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
                {
                    GUIUtility.systemCopyBuffer = LensReportBuilder.BuildReport(LensSectionRegistry.Providers);
                    state.SetStatus("Report copied.");
                }

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

            if (GUILayout.Button("Copy Debug Report", GUILayout.Width(metrics.PrimaryButtonWidth), GUILayout.MinHeight(metrics.ControlHeight)))
            {
                GUIUtility.systemCopyBuffer = LensReportBuilder.BuildReport(LensSectionRegistry.Providers);
                state.SetStatus("Report copied.");
            }

            if (state.HasStatus)
            {
                GUILayout.Label(state.StatusMessage, state.StatusIsError ? styles.ErrorStatus : styles.Status);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Toggle: {toggleKey}", styles.Status);
            GUILayout.EndHorizontal();
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
