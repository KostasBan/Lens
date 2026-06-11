using UnityEngine;

namespace KostasBan.Lens
{
    public static class LensReportMetadata
    {
        private static string projectBuildNumber = string.Empty;

        public static string ProjectBuildNumber
        {
            get => projectBuildNumber;
            set => projectBuildNumber = value ?? string.Empty;
        }

        public static void Reset()
        {
            projectBuildNumber = string.Empty;
        }

        internal static LensReportMetadataData Capture()
        {
            return new LensReportMetadataData
            {
                unityVersion = Application.unityVersion,
                appVersion = Application.version,
                platform = Application.platform.ToString(),
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                deviceType = SystemInfo.deviceType.ToString(),
                buildGuid = Application.buildGUID,
                projectBuildNumber = projectBuildNumber
            };
        }
    }

    [System.Serializable]
    internal sealed class LensReportMetadataData
    {
        public string unityVersion;
        public string appVersion;
        public string platform;
        public string deviceModel;
        public string operatingSystem;
        public string deviceType;
        public string buildGuid;
        public string projectBuildNumber;
    }
}
