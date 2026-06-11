using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KostasBan.Lens
{
    public static class LensNativeShare
    {
        public static bool ShareReport(LensReportArtifact artifact, string subject = "Lens Debug Report")
        {
            return ShareText(subject, BuildShareText(artifact));
        }

        public static bool ShareText(string subject, string text)
        {
            subject = string.IsNullOrWhiteSpace(subject) ? "Lens Debug Report" : subject;
            text = text ?? string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var intentClass = new AndroidJavaClass("android.content.Intent"))
            using (var intent = new AndroidJavaObject("android.content.Intent"))
            {
                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);

                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, subject))
                {
                    activity.Call("startActivity", chooser);
                }
            }

            return true;
#elif UNITY_IOS && !UNITY_EDITOR
            LensNativeShare_Show(subject, text);
            return true;
#else
            GUIUtility.systemCopyBuffer = text;
            return false;
#endif
        }

        private static string BuildShareText(LensReportArtifact artifact)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Lens Debug Report");

            if (artifact.HasTextPath)
            {
                builder.Append("Text Report: ");
                builder.AppendLine(artifact.TextPath);
            }

            if (artifact.HasJsonPath)
            {
                builder.Append("JSON Report: ");
                builder.AppendLine(artifact.JsonPath);
            }

            if (artifact.HasScreenshotPath)
            {
                builder.Append("Screenshot: ");
                builder.AppendLine(artifact.ScreenshotPath);
            }

            return builder.ToString();
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void LensNativeShare_Show(string subject, string text);
#endif
    }
}
