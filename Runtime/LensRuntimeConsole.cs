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

        private readonly List<LensEntry> entryBuffer = new List<LensEntry>();
        private readonly List<LensEntry> visibleEntries = new List<LensEntry>();
        private readonly LensConsoleState state = new LensConsoleState();
        private readonly LensEntryFilter filter = new LensEntryFilter();
        private readonly LensEntryDrawer entryDrawer = new LensEntryDrawer();
        private readonly LensGuiStyles styles = new LensGuiStyles();

        internal bool IsOpen => state.IsOpen;

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

            if (!state.IsOpen)
            {
                DrawFloatingButton();
                return;
            }

            DrawPanel();
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

        private void DrawPanel()
        {
            var width = Mathf.Min(Screen.width - 32f, 760f);
            var height = Mathf.Min(Screen.height - 32f, 620f);
            var rect = new Rect(16f, 16f, width, height);

            GUILayout.BeginArea(rect, GUIContent.none, styles.Panel);
            GUILayout.BeginVertical();

            DrawHeader();
            DrawSearch();

            state.ScrollPosition = GUILayout.BeginScrollView(state.ScrollPosition, GUILayout.ExpandHeight(true));

            foreach (var provider in LensSectionRegistry.Providers)
            {
                DrawProvider(provider);
            }

            GUILayout.EndScrollView();
            DrawFooter();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lens", styles.Title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUILayout.Width(72f)))
            {
                state.Close();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private void DrawSearch()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", styles.Key, GUILayout.Width(64f));

            var nextSearch = GUILayout.TextField(state.SearchText, styles.SearchField);

            if (!string.Equals(nextSearch, state.SearchText, StringComparison.Ordinal))
            {
                state.SearchText = nextSearch ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(state.SearchText) && GUILayout.Button("Clear", GUILayout.Width(58f)))
            {
                state.SearchText = string.Empty;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private void DrawProvider(ILensSectionProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            var sectionTitle = string.IsNullOrWhiteSpace(provider.SectionTitle) ? "Untitled" : provider.SectionTitle;

            entryBuffer.Clear();
            visibleEntries.Clear();

            try
            {
                foreach (var entry in provider.GetEntries())
                {
                    entryBuffer.Add(entry);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                entryBuffer.Add(new LensEntry("Error", "Provider failed while reading entries."));
            }

            var sectionMatchesSearch = filter.MatchesSection(sectionTitle, state.SearchText);

            foreach (var entry in entryBuffer)
            {
                if (!state.HasSearch || sectionMatchesSearch || EntryMatchesSearch(entry))
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

            if (GUILayout.Button($"{marker} {sectionTitle} ({visibleEntries.Count})", styles.SectionHeader))
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
                try
                {
                    entryDrawer.Draw(entry, sectionTitle, state, styles);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(entry.Key, styles.Key, GUILayout.Width(190f));
                    GUILayout.Label("Entry failed while drawing.", styles.ErrorStatus);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10f);
        }

        private bool EntryMatchesSearch(LensEntry entry)
        {
            try
            {
                return filter.MatchesEntry(entry, state.SearchText);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return true;
            }
        }

        private void DrawFooter()
        {
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy Debug Report", GUILayout.Width(160f)))
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

        private void DrawFloatingButton()
        {
            if (!showFloatingButton)
            {
                return;
            }

            state.FloatingButtonRect = GUI.Window(GetInstanceID(), state.FloatingButtonRect, DrawFloatingButtonWindow, GUIContent.none, styles.FloatingButtonWindow);
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
