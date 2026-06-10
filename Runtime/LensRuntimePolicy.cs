namespace KostasBan.Lens
{
    public static class LensRuntimePolicy
    {
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
            allowedOverride = allowed;
        }

        public static void ResetToDefault()
        {
            allowedOverride = null;
        }
    }
}
