using System.Collections.Generic;
using UnityEngine;

namespace KostasBan.Lens
{
    public abstract class LensSectionBehaviour : MonoBehaviour, ILensSectionProvider
    {
        public abstract string SectionTitle { get; }

        public abstract IEnumerable<LensEntry> GetEntries();

        protected virtual void OnEnable()
        {
            LensSectionRegistry.Register(this);
        }

        protected virtual void OnDisable()
        {
            LensSectionRegistry.Unregister(this);
        }
    }
}
