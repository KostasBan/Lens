using System;
using System.IO;
using UnityEngine;

namespace KostasBan.Lens
{
    public static class LensReportCapture
    {
        public const string DirectoryName = "LensReports";

        public static string GetReportDirectory()
        {
            return Path.Combine(Application.persistentDataPath, DirectoryName);
        }

        public static string CreateScreenshotPath(string fileNamePrefix = null)
        {
            var directory = GetReportDirectory();
            Directory.CreateDirectory(directory);

            var prefix = string.IsNullOrWhiteSpace(fileNamePrefix) ? "lens-report" : SanitizeFileNamePrefix(fileNamePrefix);
            var fileName = $"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png";
            return Path.Combine(directory, fileName);
        }

        public static LensReportScreenshot CaptureScreenshot(string fileNamePrefix = null)
        {
            var path = CreateScreenshotPath(fileNamePrefix);
            ScreenCapture.CaptureScreenshot(path);
            return new LensReportScreenshot(path);
        }

        private static string SanitizeFileNamePrefix(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = value.Trim().ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                for (var invalidIndex = 0; invalidIndex < invalid.Length; invalidIndex++)
                {
                    if (chars[i] == invalid[invalidIndex])
                    {
                        chars[i] = '-';
                        break;
                    }
                }
            }

            return new string(chars);
        }
    }
}
