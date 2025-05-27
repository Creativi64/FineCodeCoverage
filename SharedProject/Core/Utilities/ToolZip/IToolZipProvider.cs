namespace FineCodeCoverage.Core.Utilities
{
    internal interface IToolZipProvider
    {
        ZipDetails ProvideZip(string zipPrefix);
    }
}
