namespace FineCodeCoverage.Core.Utilities
{
    public interface IZipFile
    {
        void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName);
    }
}
