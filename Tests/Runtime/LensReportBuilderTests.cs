using System;
using NUnit.Framework;

namespace KostasBan.Lens.Tests
{
    public sealed class LensReportBuilderTests
    {
        [Test]
        public void BuildReportIncludesProviderEntries()
        {
            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            });

            StringAssert.Contains("Lens Debug Report", report);
            StringAssert.Contains("Lens Version: 0.5.0", report);
            StringAssert.Contains("[Build]", report);
            StringAssert.Contains("Version: 1.2.3", report);
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
        public void BuildReportRedactsSensitiveValues()
        {
            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Session", new LensEntry("Token", "secret-token", true))
            });

            StringAssert.Contains("Token: [redacted]", report);
            Assert.IsFalse(report.Contains("secret-token"));
        }

        [Test]
        public void BuildReportBubblesProviderFailures()
        {
            Assert.Throws<InvalidOperationException>(() => LensReportBuilder.BuildReport(new[]
            {
                new ThrowingProvider()
            }));
        }

        [Test]
        public void BuildReportIncludesInfoText()
        {
            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Info", LensEntry.ReadOnly("Flag", "Enabled", "Shown to explain a value."))
            });

            StringAssert.Contains("Flag: Enabled", report);
            StringAssert.Contains("Info: Shown to explain a value.", report);
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
