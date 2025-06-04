using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IToolZipProvider))]
    internal class ToolZipProvider : IToolZipProvider
    {
        internal const string ZippedToolsDirectoryName = "ZippedTools";
        internal string ExtensionDirectory { get; set; }
        public ToolZipProvider()
            => this.ExtensionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public ZipDetails ProvideZip(string zipPrefix)
        {
            string zipFolder = Path.Combine(this.ExtensionDirectory, ZippedToolsDirectoryName);
            string[] matchingZipFiles = Directory.GetFiles(zipFolder, $"{zipPrefix}.*.zip");
            string zipPath = matchingZipFiles[0];

            string zipFileName = Path.GetFileName(zipPath);
            string version = zipFileName.Replace($"{zipPrefix}.", "").Replace(".zip", "");

            return new ZipDetails { Path = zipPath, Version = version };
        }
    }
}
