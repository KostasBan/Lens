using UnityEngine;

namespace KostasBan.Lens
{
    internal readonly struct LensLayoutMetrics
    {
        private const float CompactWidthThreshold = 560f;
        private const float PortraitWidthThreshold = 700f;

        private LensLayoutMetrics(float uiScale, float logicalScreenWidth, float logicalScreenHeight)
        {
            UiScale = uiScale;
            LogicalScreenWidth = logicalScreenWidth;
            LogicalScreenHeight = logicalScreenHeight;
            IsPortrait = logicalScreenHeight >= logicalScreenWidth;
            IsCompact = logicalScreenWidth < CompactWidthThreshold || (IsPortrait && logicalScreenWidth < PortraitWidthThreshold);
        }

        public float UiScale { get; }

        public float LogicalScreenWidth { get; }

        public float LogicalScreenHeight { get; }

        public bool IsCompact { get; }

        public bool IsPortrait { get; }

        public float Margin => IsCompact ? 8f : 16f;

        public float PanelMaxWidth => IsCompact ? LogicalScreenWidth - (Margin * 2f) : Mathf.Min(LogicalScreenWidth - (Margin * 2f), 760f);

        public float PanelMaxHeight => IsCompact ? LogicalScreenHeight - (Margin * 2f) : Mathf.Min(LogicalScreenHeight - (Margin * 2f), 620f);

        public float KeyWidth => IsCompact ? 0f : 190f;

        public float SearchLabelWidth => IsCompact ? 0f : 64f;

        public float PrimaryButtonWidth => IsCompact ? 132f : 160f;

        public float SmallButtonWidth => IsCompact ? 76f : 72f;

        public float ApplyButtonWidth => IsCompact ? 74f : 58f;

        public float RevertButtonWidth => IsCompact ? 82f : 62f;

        public float ValueFieldWidth => IsCompact ? 150f : 120f;

        public float SliderWidth => IsCompact ? 180f : 140f;

        public float OptionPopupWidth => IsCompact ? Mathf.Min(LogicalScreenWidth - 48f, 320f) : 240f;

        public float InfoButtonWidth => 34f;

        public float ControlHeight => IsCompact ? 42f : 24f;

        public Rect PanelRect => new Rect(Margin, Margin, Mathf.Max(1f, PanelMaxWidth), Mathf.Max(1f, PanelMaxHeight));

        public static LensLayoutMetrics FromScreen(
            int screenWidth,
            int screenHeight,
            float dpi,
            LensUiScaleMode scaleMode,
            float fixedScale,
            float minAutoScale,
            float maxAutoScale)
        {
            var scale = CalculateScale(screenWidth, screenHeight, dpi, scaleMode, fixedScale, minAutoScale, maxAutoScale);
            return new LensLayoutMetrics(scale, Mathf.Max(1f, screenWidth / scale), Mathf.Max(1f, screenHeight / scale));
        }

        public static float CalculateScale(
            int screenWidth,
            int screenHeight,
            float dpi,
            LensUiScaleMode scaleMode,
            float fixedScale,
            float minAutoScale,
            float maxAutoScale)
        {
            if (scaleMode == LensUiScaleMode.Fixed)
            {
                return Mathf.Clamp(fixedScale <= 0f ? 1f : fixedScale, 0.5f, 4f);
            }

            var minScale = Mathf.Max(0.5f, minAutoScale);
            var maxScale = Mathf.Max(minScale, maxAutoScale);
            var shortestSide = Mathf.Min(Mathf.Max(1, screenWidth), Mathf.Max(1, screenHeight));
            var isPortrait = screenHeight >= screenWidth;

            var scale = 1f;
            if (dpi >= 120f)
            {
                scale = dpi / 160f;
            }
            else if (isPortrait && shortestSide >= 720)
            {
                scale = shortestSide / 480f;
            }

            return Mathf.Clamp(scale, minScale, maxScale);
        }

        public Rect ClampFloatingButton(Rect rect)
        {
            var width = Mathf.Max(IsCompact ? 96f : 72f, rect.width);
            var height = Mathf.Max(IsCompact ? 48f : 36f, rect.height);
            var maxX = Mathf.Max(0f, LogicalScreenWidth - width - Margin);
            var maxY = Mathf.Max(0f, LogicalScreenHeight - height - Margin);
            var minX = Mathf.Min(Margin, maxX);
            var minY = Mathf.Min(Margin, maxY);

            return new Rect(
                Mathf.Clamp(rect.x, minX, maxX),
                Mathf.Clamp(rect.y, minY, maxY),
                width,
                height);
        }
    }
}
