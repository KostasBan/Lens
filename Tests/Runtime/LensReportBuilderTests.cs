using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace KostasBan.Lens.Tests
{
    public sealed class LensReportBuilderTests
    {
        [TearDown]
        public void TearDown()
        {
            LensReportMetadata.Reset();
        }

        [Test]
        public void BuildReportIncludesProviderEntries()
        {
            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            });

            StringAssert.Contains("Lens Debug Report", report);
            StringAssert.Contains("Report Schema: 1", report);
            StringAssert.Contains("Lens Version: 1.0.0", report);
            StringAssert.Contains("Unity Version:", report);
            StringAssert.Contains("App Version:", report);
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

        [Test]
        public void BuildJsonReportIncludesProviderEntries()
        {
            LensReportMetadata.ProjectBuildNumber = "build-42";
            var report = LensReportBuilder.BuildJsonReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            });

            StringAssert.Contains("\"schemaVersion\": 1", report);
            StringAssert.Contains("\"lensVersion\": \"1.0.0\"", report);
            StringAssert.Contains("\"metadata\":", report);
            StringAssert.Contains("\"projectBuildNumber\": \"build-42\"", report);
            StringAssert.Contains("\"title\": \"Build\"", report);
            StringAssert.Contains("\"key\": \"Version\"", report);
            StringAssert.Contains("\"kind\": \"ReadOnly\"", report);
            StringAssert.Contains("\"value\": \"1.2.3\"", report);
        }

        [Test]
        public void BuildJsonReportRedactsSensitiveValues()
        {
            var report = LensReportBuilder.BuildJsonReport(new[]
            {
                new StaticProvider("Session", new LensEntry("Token", "secret-token", true))
            });

            StringAssert.Contains("\"value\": \"[redacted]\"", report);
            StringAssert.Contains("\"isSensitive\": true", report);
            Assert.IsFalse(report.Contains("secret-token"));
        }

        [Test]
        public void BuildJsonReportListsActionsWithoutExecutingThem()
        {
            var executed = false;

            var report = LensReportBuilder.BuildJsonReport(new[]
            {
                new StaticProvider("Actions", LensEntry.Button("Unlock Content", () => executed = true))
            });

            Assert.IsFalse(executed);
            StringAssert.Contains("\"isAction\": true", report);
            StringAssert.Contains("\"value\": \"[Action] Unlock Content\"", report);
        }

        [Test]
        public void BuildJsonReportIncludesInfoText()
        {
            var report = LensReportBuilder.BuildJsonReport(new[]
            {
                new StaticProvider("Info", LensEntry.ReadOnly("Flag", "Enabled", "Shown to explain a value."))
            });

            StringAssert.Contains("\"info\": \"Shown to explain a value.\"", report);
        }

        [Test]
        public void BuildReportsIncludeScreenshotPathMetadata()
        {
            var screenshot = new LensReportScreenshot("C:/Temp/lens-report.png");

            var textReport = LensReportBuilder.BuildTextReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            }, screenshot);
            var jsonReport = LensReportBuilder.BuildJsonReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            }, true, screenshot);

            StringAssert.Contains("Screenshot: C:/Temp/lens-report.png", textReport);
            StringAssert.Contains("\"screenshotPath\": \"C:/Temp/lens-report.png\"", jsonReport);
        }

        [Test]
        public void ExportWritesTextAndJsonReports()
        {
            var artifact = LensReportExporter.Export(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            }, false, "lens-test");

            try
            {
                Assert.IsTrue(File.Exists(artifact.TextPath));
                Assert.IsTrue(File.Exists(artifact.JsonPath));
                Assert.IsFalse(artifact.HasScreenshotPath);
                StringAssert.Contains("Version: 1.2.3", File.ReadAllText(artifact.TextPath));
                StringAssert.Contains("\"value\": \"1.2.3\"", File.ReadAllText(artifact.JsonPath));
            }
            finally
            {
                if (File.Exists(artifact.TextPath))
                {
                    File.Delete(artifact.TextPath);
                }

                if (File.Exists(artifact.JsonPath))
                {
                    File.Delete(artifact.JsonPath);
                }
            }
        }

        [Test]
        public void NativeShareFallsBackToClipboardInEditor()
        {
            var shared = LensNativeShare.ShareText("Lens", "Fallback report");

            Assert.IsFalse(shared);
            Assert.AreEqual("Fallback report", GUIUtility.systemCopyBuffer);
        }

        [Test]
        public void BuildReportSupportsJsonFormat()
        {
            var report = LensReportBuilder.BuildReport(new[]
            {
                new StaticProvider("Build", new LensEntry("Version", "1.2.3"))
            }, LensReportFormat.Json);

            StringAssert.Contains("\"sections\"", report);
            StringAssert.Contains("\"value\": \"1.2.3\"", report);
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
