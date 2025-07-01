namespace FineCodeCoverage.Utilities.Wrappers
{
    public interface IZipFile
    {
        void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName);
    }
}
