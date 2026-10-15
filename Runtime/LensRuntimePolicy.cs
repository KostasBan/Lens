namespace KostasBan.Lens
{
    public static class LensRuntimePolicy
    {
        public static event System.Action<bool> AllowedChanged;
        private static bool? allowedOverride;

        public static bool DefaultIsAllowed
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LENS_ENABLED
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsAllowed => allowedOverride ?? DefaultIsAllowed;

        public static void SetAllowed(bool allowed)
        {
            var previous = IsAllowed;
            allowedOverride = allowed;
            if (previous != IsAllowed) AllowedChanged?.Invoke(IsAllowed);
        }

        public static void ResetToDefault()
        {
            var previous = IsAllowed;
            allowedOverride = null;
            if (previous != IsAllowed) AllowedChanged?.Invoke(IsAllowed);
        }
    }
}
