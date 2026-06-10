using System;
using System.Collections.Generic;

namespace KostasBan.Lens
{
    public static class LensEntryDrawerRegistry
    {
        private static readonly Dictionary<string, ILensEntryDrawer> Drawers = new Dictionary<string, ILensEntryDrawer>();

        public static void Register(string typeId, ILensEntryDrawer drawer)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                throw new ArgumentException("Custom entry type id is required.", nameof(typeId));
            }

            if (drawer == null)
            {
                throw new ArgumentNullException(nameof(drawer));
            }

            Drawers[typeId] = drawer;
        }

        public static void Unregister(string typeId)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                return;
            }

            Drawers.Remove(typeId);
        }

        public static bool TryGet(string typeId, out ILensEntryDrawer drawer)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                drawer = null;
                return false;
            }

            return Drawers.TryGetValue(typeId, out drawer);
        }

        public static ILensEntryDrawer GetRequired(string typeId)
        {
            if (TryGet(typeId, out var drawer))
            {
                return drawer;
            }

            throw new InvalidOperationException($"No Lens entry drawer registered for custom type '{typeId}'.");
        }

        public static void Clear()
        {
            Drawers.Clear();
        }
    }
}
