using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KostasBan.Lens
{
    public static class LensReportBuilder
    {
        public const int SchemaVersion = 1;

        public static string BuildReport(IEnumerable<ILensSectionProvider> providers)
        {
            return BuildTextReport(providers);
        }

        public static string BuildReport(IEnumerable<ILensSectionProvider> providers, LensReportFormat format)
        {
            return BuildReport(providers, format, LensReportOptions.Default);
        }

        public static string BuildReport(IEnumerable<ILensSectionProvider> providers, LensReportFormat format, LensReportOptions options)
        {
            var screenshot = options.IncludeScreenshot ? LensReportCapture.CaptureScreenshot(options.ScreenshotFileNamePrefix) : default;

            switch (format)
            {
                case LensReportFormat.Text:
                    return BuildTextReport(providers, screenshot);
                case LensReportFormat.Json:
                    return BuildJsonReport(providers, options.PrettyJson, screenshot);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported Lens report format.");
            }
        }

        public static string BuildTextReport(IEnumerable<ILensSectionProvider> providers)
        {
            return BuildTextReport(providers, default);
        }

        public static string BuildTextReport(IEnumerable<ILensSectionProvider> providers, LensReportScreenshot screenshot)
        {
            return BuildTextReport(CreateReportData(providers, screenshot));
        }

        public static string BuildJsonReport(IEnumerable<ILensSectionProvider> providers)
        {
            return BuildJsonReport(providers, true);
        }

        public static string BuildJsonReport(IEnumerable<ILensSectionProvider> providers, bool prettyPrint)
        {
            return BuildJsonReport(providers, prettyPrint, default);
        }

        public static string BuildJsonReport(IEnumerable<ILensSectionProvider> providers, bool prettyPrint, LensReportScreenshot screenshot)
        {
            return JsonUtility.ToJson(CreateReportData(providers, screenshot), prettyPrint);
        }

        private static string BuildTextReport(LensReportData report)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Lens Debug Report");
            builder.Append("Report Schema: ");
            builder.AppendLine(report.schemaVersion.ToString());
            builder.Append("Generated: ");
            builder.AppendLine(report.generatedUtc);
            builder.Append("Lens Version: ");
            builder.AppendLine(report.lensVersion);
            AppendMetadata(builder, report.metadata);

            if (!string.IsNullOrWhiteSpace(report.screenshotPath))
            {
                builder.Append("Screenshot: ");
                builder.AppendLine(report.screenshotPath);
            }

            builder.AppendLine();

            for (var sectionIndex = 0; sectionIndex < report.sections.Count; sectionIndex++)
            {
                var section = report.sections[sectionIndex];
                builder.Append('[');
                builder.Append(section.title);
                builder.AppendLine("]");

                for (var entryIndex = 0; entryIndex < section.entries.Count; entryIndex++)
                {
                    AppendEntry(builder, section.entries[entryIndex]);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static LensReportData CreateReportData(IEnumerable<ILensSectionProvider> providers, LensReportScreenshot screenshot)
        {
            var report = new LensReportData
            {
                schemaVersion = SchemaVersion,
                generatedUtc = DateTime.UtcNow.ToString("O"),
                lensVersion = LensPackageInfo.Version,
                screenshotPath = screenshot.HasPath ? screenshot.Path : string.Empty,
                metadata = LensReportMetadata.Capture()
            };

            if (providers == null)
            {
                return report;
            }

            foreach (var provider in providers)
            {
                if (provider == null)
                {
                    continue;
                }

                report.sections.Add(CreateSectionData(provider));
            }

            return report;
        }

        private static LensReportSectionData CreateSectionData(ILensSectionProvider provider)
        {
            var section = new LensReportSectionData
            {
                title = string.IsNullOrWhiteSpace(provider.SectionTitle) ? "Untitled" : provider.SectionTitle
            };

            foreach (var entry in provider.GetEntries())
            {
                section.entries.Add(CreateEntryData(entry));
            }

            return section;
        }

        private static LensReportEntryData CreateEntryData(LensEntry entry)
        {
            return new LensReportEntryData
            {
                key = string.IsNullOrWhiteSpace(entry.Key) ? "(empty)" : entry.Key,
                kind = entry.Kind.ToString(),
                value = entry.ReportValue,
                isSensitive = entry.IsSensitive,
                isAction = entry.Kind == LensEntryKind.Button,
                info = entry.HasInfo ? entry.InfoText : string.Empty
            };
        }

        private static void AppendEntry(StringBuilder builder, LensReportEntryData entry)
        {
            builder.Append(entry.key);
            builder.Append(": ");
            builder.AppendLine(entry.value);

            if (!string.IsNullOrWhiteSpace(entry.info))
            {
                builder.Append("  Info: ");
                builder.AppendLine(entry.info);
            }
        }

        private static void AppendMetadata(StringBuilder builder, LensReportMetadataData metadata)
        {
            if (metadata == null)
            {
                return;
            }

            builder.Append("Unity Version: ");
            builder.AppendLine(metadata.unityVersion);
            builder.Append("App Version: ");
            builder.AppendLine(metadata.appVersion);
            builder.Append("Platform: ");
            builder.AppendLine(metadata.platform);
            builder.Append("Device Model: ");
            builder.AppendLine(metadata.deviceModel);
            builder.Append("Operating System: ");
            builder.AppendLine(metadata.operatingSystem);
            builder.Append("Device Type: ");
            builder.AppendLine(metadata.deviceType);
            builder.Append("Build GUID: ");
            builder.AppendLine(metadata.buildGuid);

            if (!string.IsNullOrWhiteSpace(metadata.projectBuildNumber))
            {
                builder.Append("Project Build Number: ");
                builder.AppendLine(metadata.projectBuildNumber);
            }
        }

        [Serializable]
        private sealed class LensReportData
        {
            public int schemaVersion;
            public string generatedUtc;
            public string lensVersion;
            public string screenshotPath;
            public LensReportMetadataData metadata;
            public List<LensReportSectionData> sections = new List<LensReportSectionData>();
        }

        [Serializable]
        private sealed class LensReportSectionData
        {
            public string title;
            public List<LensReportEntryData> entries = new List<LensReportEntryData>();
        }

        [Serializable]
        private sealed class LensReportEntryData
        {
            public string key;
            public string kind;
            public string value;
            public bool isSensitive;
            public bool isAction;
            public string info;
        }
    }
}
