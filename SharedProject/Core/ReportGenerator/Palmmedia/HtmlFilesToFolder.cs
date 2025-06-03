using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IHtmlFilesToFolder))]
    internal class HtmlFilesToFolder : IHtmlFilesToFolder
    {
        public void Collate(string reportOutputFolder)
        {
            DeleteHtm(reportOutputFolder);
            Collate(reportOutputFolder, "HtmlReport");
        }

        private static void Collate(string reportOutputFolder, string directoryName)
        {
            var directory = new DirectoryInfo(reportOutputFolder);
            DirectoryInfo htmlReportDirectory = directory.CreateSubdirectory(directoryName);

            foreach (FileInfo htmlFile in directory.EnumerateFiles("*.html"))
            {
                htmlFile.MoveTo(Path.Combine(htmlReportDirectory.FullName, htmlFile.Name));
            }
        }

        private static void DeleteHtm(string reportOutputFolder)
        {
            string path = Path.Combine(reportOutputFolder, "index.htm");
            File.Delete(path);
        }
    }
}