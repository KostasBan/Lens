using UnityEngine;

namespace KostasBan.Lens
{
    internal sealed class LensGuiStyles
    {
        private readonly Color panelColor = new Color(0.05f, 0.05f, 0.06f, 0.92f);

        private Texture2D panelTexture;

        public GUIStyle Panel { get; private set; }

        public GUIStyle Title { get; private set; }

        public GUIStyle SectionHeader { get; private set; }

        public GUIStyle Key { get; private set; }

        public GUIStyle Value { get; private set; }

        public GUIStyle Status { get; private set; }

        public GUIStyle ErrorStatus { get; private set; }

        public GUIStyle SearchField { get; private set; }

        public GUIStyle FloatingButtonWindow { get; private set; }

        public void EnsureInitialized()
        {
            if (Panel != null)
            {
                return;
            }

            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, panelColor);
            panelTexture.Apply();
            panelTexture.hideFlags = HideFlags.HideAndDontSave;

            Panel = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 12, 12),
                normal = { background = panelTexture }
            };

            Title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            SectionHeader = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.56f, 0.84f, 1f) }
            };

            Key = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.9f, 0.92f) }
            };

            Value = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            Status = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(0.72f, 0.92f, 0.72f) }
            };

            ErrorStatus = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(1f, 0.58f, 0.52f) }
            };

            SearchField = new GUIStyle(GUI.skin.textField);

            FloatingButtonWindow = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(4, 4, 4, 4)
            };
        }
    }
}
