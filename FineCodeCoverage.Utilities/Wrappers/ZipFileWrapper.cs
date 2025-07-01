using System.ComponentModel.Composition;
using System.IO.Compression;

namespace FineCodeCoverage.Utilities.Wrappers
{
    [Export(typeof(IZipFile))]
    internal sealed class ZipFileWrapper : IZipFile
    {
        public void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
            => ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);
    }
}
