using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Initialization.ToolZip
{
    [Export(typeof(IToolFolder))]
    internal sealed class ToolFolder : IToolFolder
    {
        private readonly IZipFile _zipFile;

        [ImportingConstructor]
        public ToolFolder(IZipFile zipFile) => _zipFile = zipFile;

        public string EnsureUnzipped(
            string appDataFolder,
            string toolFolderName,
            ZipDetails zipDetails,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string version = zipDetails.Version;

            string toolFolderPath = Path.Combine(appDataFolder, toolFolderName);
            DirectoryInfo toolDirectory = Directory.CreateDirectory(toolFolderPath);
            string zipDestination = Path.Combine(toolFolderPath, version);

            cancellationToken.ThrowIfCancellationRequested();
            DirectoryInfo[] unzippedDirectories = toolDirectory.GetDirectories();
            bool requiresUnzip = !unzippedDirectories.Any(d => d.Name == version);

            if (requiresUnzip)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (FileInfo file in toolDirectory.GetFiles())
                {
                    file.TryDelete();
                }

                foreach (DirectoryInfo unzippedDirectory in unzippedDirectories)
                {
                    unzippedDirectory.TryDelete();
                }

                _ = Directory.CreateDirectory(zipDestination);
                _zipFile.ExtractToDirectory(zipDetails.Path, zipDestination);
            }

            return zipDestination;
        }
    }
}
