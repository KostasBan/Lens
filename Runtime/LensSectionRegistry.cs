using System;
using System.Collections.Generic;

namespace KostasBan.Lens
{
    public static class LensSectionRegistry
    {
        private static readonly List<ILensSectionProvider> RegisteredProviders = new List<ILensSectionProvider>();

        public static IReadOnlyList<ILensSectionProvider> Providers => RegisteredProviders;

        public static void Register(ILensSectionProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (!RegisteredProviders.Contains(provider))
            {
                RegisteredProviders.Add(provider);
            }
        }

        public static void Unregister(ILensSectionProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            RegisteredProviders.Remove(provider);
        }

        public static void Clear()
        {
            RegisteredProviders.Clear();
        }
    }
}
