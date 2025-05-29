using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletDataCollectorGeneratedCobertura))]
    internal class CoverletDataCollectorGeneratedCobertura : ICoverletDataCollectorGeneratedCobertura
    {
        internal const string collectorGeneratedCobertura = "coverage.cobertura.xml";
        private FileInfo GetCoberturaFile(string coverageOutputFolder)
        {
            //C:\\Users\\tonyh\\Source\\Repos\\DataCollectorXUnit\\XUnitTestProject1\\bin\\Debug\\netcoreapp3.1\\fine-code-coverage\\coverage-tool-output\\7ba6447d-a89f-4836-bffc-aeb4799e48ab\\coverage.cobertura.xml\r\nP
            var coverageOutputDirectory = new DirectoryInfo(coverageOutputFolder);
            System.Collections.Generic.List<FileInfo> generatedCoberturaFiles = coverageOutputDirectory.GetFiles(collectorGeneratedCobertura, SearchOption.AllDirectories).ToList();
            //should only be the one
            FileInfo lastWrittenCobertura = generatedCoberturaFiles.OrderBy(f => f.LastWriteTime).LastOrDefault();
            return lastWrittenCobertura;
        }
        public void CorrectPath(string coverageOutputFolder, string coverageOutputFile)
        {
            FileInfo coberturaFile = this.GetCoberturaFile(coverageOutputFolder) ?? throw new Exception($"Data collector did not generate {collectorGeneratedCobertura}");
            DirectoryInfo guidDirectoryToDelete = coberturaFile.Directory;
            coberturaFile.MoveTo(coverageOutputFile);

            guidDirectoryToDelete.TryDelete();
        }
    }
}
