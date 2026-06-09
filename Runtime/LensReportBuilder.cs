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

            try
            {
                foreach (var entry in provider.GetEntries())
                {
                    builder.Append(string.IsNullOrWhiteSpace(entry.Key) ? "(empty)" : entry.Key);
                    builder.Append(": ");
                    builder.AppendLine(entry.Value);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                builder.AppendLine("Error: Provider failed while generating report.");
            }

            builder.AppendLine();
        }
    }
}
