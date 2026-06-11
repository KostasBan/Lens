using System.Collections.Generic;
using System.IO;

namespace KostasBan.Lens
{
    public static class LensReportExporter
    {
        public static LensReportArtifact Export(IEnumerable<ILensSectionProvider> providers, bool includeScreenshot = true, string fileNamePrefix = null)
        {
            var screenshot = includeScreenshot ? LensReportCapture.CaptureScreenshot(fileNamePrefix) : default;
            var textPath = LensReportCapture.CreateReportPath("txt", fileNamePrefix);
            var jsonPath = LensReportCapture.CreateReportPath("json", fileNamePrefix);

            File.WriteAllText(textPath, LensReportBuilder.BuildTextReport(providers, screenshot));
            File.WriteAllText(jsonPath, LensReportBuilder.BuildJsonReport(providers, true, screenshot));

            return new LensReportArtifact(textPath, jsonPath, screenshot.HasPath ? screenshot.Path : string.Empty);
        }
    }
}
