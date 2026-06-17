using KostasBan.Lens;
using UnityEngine;

public sealed class LensDemoBootstrap : MonoBehaviour
{
    private readonly ILensSectionProvider[] providers =
    {
        new BuildInfoLensSectionProvider(),
        new SessionInfoLensSectionProvider(),
        new SampleFeatureFlagsLensSectionProvider(),
        new SampleRecentEventsLensSectionProvider(),
        new PerformanceLensSectionProvider()
    };

    private void Awake()
    {
        if (!LensRuntimePolicy.IsAllowed)
        {
            return;
        }

        LensSectionRegistry.Clear();

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
