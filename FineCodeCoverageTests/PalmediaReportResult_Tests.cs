using Cobertura;
using FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Utilities.Wrappers;
using Moq;
using Palmmedia.ReportGenerator.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FineCodeCoverageTests
{
    internal class PalmediaTestReportResult
    {
        public PalmediaTestReportResult(PalmmediaReportResult palmediaReportResult, string tempDirectory)
        {
            PalmediaReportResult = palmediaReportResult;
            TempDirectory = tempDirectory;
        }

        public PalmmediaReportResult PalmediaReportResult { get; }
        public string TempDirectory { get; }
    }

    internal static class PalmmediaTestReportGenerator
    {
        public static string TempDirectory { get; private set; }
        public static PalmmediaReportResult GenerateFromCobertura(Func<string,string> generateCobertura)
        {
            var reportGenerator = new PalmmediaReportGenerator(new Mock<IHtmlFilesToFolder>().Object);
            TempDirectory = new FileUtil().CreateTempDirectory();
            var cobertura = generateCobertura(TempDirectory);
            var coberturaFilePath = WriteCobertura(cobertura);
            
            LoggerFactory.Configure((palmmediaVerbosityLevel, message) =>
            {

            });
            LoggerFactory.VerbosityLevel = Palmmedia.ReportGenerator.Core.Logging.VerbosityLevel.Verbose;
            var result =  reportGenerator.Generate(new List<string> { coberturaFilePath }, TempDirectory, new List<string> { "Cobertura" });
            return (PalmmediaReportResult)result;
        }

        public static PalmmediaReportResult GenerateFromCoberturaPath(Func<string,string> generateCoberturaFilePath)
        {
            Func<string,string> generateCobertura = (tmpDirPath) => File.ReadAllText(generateCoberturaFilePath(tmpDirPath));
            return GenerateFromCobertura(generateCobertura);
        }

        public static PalmmediaReportResult GenerateFromCoberturaReport(Func<string,CoberturaReport> generateCoberturaReport)
        {
            Func<string, string> generateCobertura = (tmpDirPath) =>
            {
                var coberturaReport = generateCoberturaReport(tmpDirPath);
                var stringBuilder = new StringBuilder();
                using (var xmlWriter = XmlWriter.Create(stringBuilder))
                {
                    var xmlSerializer = new XmlSerializer(typeof(CoberturaReport));

                    xmlSerializer.Serialize(xmlWriter, coberturaReport);
                }
                var cobertura = stringBuilder.ToString();
                // Insert DOCTYPE after the XML declaration
                string doctype = "<!DOCTYPE coverage SYSTEM \"http://cobertura.sourceforge.net/xml/coverage-04.dtd\">";
                int index = cobertura.IndexOf("?>") + 2;
                cobertura = cobertura.Insert(index, Environment.NewLine + doctype);
                return cobertura;
            };
            

            return GenerateFromCobertura(generateCobertura);
        }

        private static string WriteCobertura(string cobertura)
        {
            var coberturaPath = Path.Combine(TempDirectory, "initial-cobertura.xml");
            File.WriteAllText(coberturaPath, cobertura, Encoding.Unicode);
            return coberturaPath;
        }
    }
}
