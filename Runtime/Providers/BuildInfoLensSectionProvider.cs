using UnityEngine;
using UnityEngine.SceneManagement;

namespace KostasBan.Lens
{
    public sealed class BuildInfoLensSectionProvider : ILensSectionProvider
    {
        public string SectionTitle => "Build Info";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            yield return new LensEntry("App Version", Application.version);
            yield return new LensEntry("Unity Version", Application.unityVersion);
            yield return new LensEntry("Platform", Application.platform.ToString());
            yield return new LensEntry("Active Scene", SceneManager.GetActiveScene().name);
            yield return new LensEntry("Debug Build", Debug.isDebugBuild.ToString());
        }
    }
}
