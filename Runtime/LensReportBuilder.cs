using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KostasBan.Lens
{
    public static class LensReportBuilder
    {
        public static string BuildReport(IEnumerable<ILensSectionProvider> providers)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Lens Debug Report");
            builder.Append("Generated: ");
            builder.AppendLine(DateTime.UtcNow.ToString("O"));
            builder.Append("Lens Version: ");
            builder.AppendLine(LensPackageInfo.Version);
            builder.AppendLine();

            if (providers == null)
            {
                return builder.ToString();
            }

            foreach (var provider in providers)
            {
                if (provider == null)
                {
                    continue;
                }

                AppendProvider(builder, provider);
            }

            return builder.ToString();
        }

        private static void AppendProvider(StringBuilder builder, ILensSectionProvider provider)
        {
            builder.Append('[');
            builder.Append(string.IsNullOrWhiteSpace(provider.SectionTitle) ? "Untitled" : provider.SectionTitle);
            builder.AppendLine("]");

            foreach (var entry in provider.GetEntries())
            {
                AppendEntry(builder, entry);
            }

            builder.AppendLine();
        }

        private static void AppendEntry(StringBuilder builder, LensEntry entry)
        {
            builder.Append(string.IsNullOrWhiteSpace(entry.Key) ? "(empty)" : entry.Key);
            builder.Append(": ");
            builder.AppendLine(entry.ReportValue);

            if (entry.HasInfo)
            {
                builder.Append("  Info: ");
                builder.AppendLine(entry.InfoText);
            }
        }
    }
}
