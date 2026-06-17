using KostasBan.Lens;
using UnityEngine;

namespace KostasBan.Lens.Samples
{
    public sealed class LensCookbookBootstrap : MonoBehaviour
    {
        private readonly ILensSectionProvider[] providers =
        {
            new FakeRemoteConfigLensSectionProvider(),
            new FakeAnalyticsLensSectionProvider(),
            new FakeContentUnlockLensSectionProvider()
        };

        private void Awake()
        {
            if (!LensRuntimePolicy.IsAllowed)
            {
                return;
            }

            foreach (var provider in providers)
            {
                LensSectionRegistry.Register(provider);
            }

            LensRuntimeConsole.EnsureExists();
        }

        private void OnDestroy()
        {
            foreach (var provider in providers)
            {
                LensSectionRegistry.Unregister(provider);
            }
        }
    }
}
