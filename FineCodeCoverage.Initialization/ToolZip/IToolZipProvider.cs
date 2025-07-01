namespace FineCodeCoverage.Initialization.ToolZip
{
    internal interface IToolZipProvider
    {
        ZipDetails ProvideZip(string zipPrefix);
    }
}
