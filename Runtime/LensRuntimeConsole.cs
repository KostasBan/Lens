using System;
using System.Collections.Generic;
using UnityEngine;

namespace KostasBan.Lens
{
    public sealed class LensRuntimeConsole : MonoBehaviour
    {
        private const float CopyStatusDuration = 2f;

        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        private readonly List<LensEntry> entryBuffer = new List<LensEntry>();
        private readonly Color panelColor = new Color(0.05f, 0.05f, 0.06f, 0.92f);

        private bool isOpen;
        private float copiedAt = -100f;
        private Vector2 scrollPosition;
        private Texture2D panelTexture;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle keyStyle;
        private GUIStyle valueStyle;
        private GUIStyle statusStyle;

        private void Awake()
        {
            // Lens is intended for Editor, Development Builds, or explicit LENS_ENABLED builds.
            // A later version can wrap initialization with UNITY_EDITOR || DEVELOPMENT_BUILD || LENS_ENABLED.
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isOpen = !isOpen;
            }
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            EnsureStyles();
            DrawPanel();
        }

        private void DrawPanel()
        {
            var width = Mathf.Min(Screen.width - 32f, 760f);
            var height = Mathf.Min(Screen.height - 32f, 620f);
            var rect = new Rect(16f, 16f, width, height);

            GUILayout.BeginArea(rect, GUIContent.none, panelStyle);
            GUILayout.BeginVertical();

            DrawHeader();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

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
            GUILayout.Label("Lens", titleStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUILayout.Width(72f)))
            {
                isOpen = false;
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

            GUILayout.Label(string.IsNullOrWhiteSpace(provider.SectionTitle) ? "Untitled" : provider.SectionTitle, sectionStyle);

            entryBuffer.Clear();

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

            foreach (var entry in entryBuffer)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Key, keyStyle, GUILayout.Width(190f));
                GUILayout.Label(entry.Value, valueStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
        }

        private void DrawFooter()
        {
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy Debug Report", GUILayout.Width(160f)))
            {
                GUIUtility.systemCopyBuffer = LensReportBuilder.BuildReport(LensSectionRegistry.Providers);
                copiedAt = Time.realtimeSinceStartup;
            }

            if (Time.realtimeSinceStartup - copiedAt <= CopyStatusDuration)
            {
                GUILayout.Label("Report copied.", statusStyle);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Toggle: {toggleKey}", statusStyle);
            GUILayout.EndHorizontal();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, panelColor);
            panelTexture.Apply();
            panelTexture.hideFlags = HideFlags.HideAndDontSave;

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 12, 12),
                normal = { background = panelTexture }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            sectionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.56f, 0.84f, 1f) }
            };

            keyStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.9f, 0.92f) }
            };

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            statusStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(0.72f, 0.92f, 0.72f) }
            };
        }
    }
}
