using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KostasBan.Lens.Tests
{
    public sealed class LensReportBuilderTests
    {
        [Test]
        public void BuildReportIncludesProviderEntries()
        {
            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "0.3.0"))
            });

            StringAssert.Contains("Lens Debug Report", report);
            StringAssert.Contains("[Build]", report);
            StringAssert.Contains("Version: 0.3.0", report);
        }

        [Test]
        public void BuildReportIncludesCurrentMutableValues()
        {
            var enabled = true;
            var multiplier = 2.5f;

            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider(
                    "Mutable",
                    LensEntry.Toggle("Enabled", () => enabled, next => enabled = next),
                    LensEntry.Number("Multiplier", () => multiplier, next => multiplier = next))
            });

            StringAssert.Contains("Enabled: True", report);
            StringAssert.Contains("Multiplier: 2.5", report);
        }

        [Test]
        public void BuildReportListsActionsWithoutExecutingThem()
        {
            var executed = false;

            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Actions", LensEntry.Button("Unlock Content", () => executed = true))
            });

            Assert.IsFalse(executed);
            StringAssert.Contains("Unlock Content: [Action] Unlock Content", report);
        }

        [Test]
        public void BuildReportHandlesProviderFailures()
        {
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: Broken provider.");

            var report = LensReportBuilder.BuildReport(new[]
            {
                new ThrowingProvider()
            });

            StringAssert.Contains("[Broken]", report);
            StringAssert.Contains("Error: Provider failed while generating report.", report);
        }

        private sealed class ThrowingProvider : ILensSectionProvider
        {
            public string SectionTitle => "Broken";

            public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
            {
                throw new InvalidOperationException("Broken provider.");
            }
        }
    }
}
