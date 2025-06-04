namespace FineCodeCoverage.Core.Utilities
{
    internal static class FCCGithub
    {
        internal const string Repo = "https://github.com/FortuneN/FineCodeCoverage";
        internal static string MasterBlob { get; } = $"{Repo}/blob/master/";
        internal static string Readme { get; } = $"{MasterBlob}/README.md";
        internal static string Issues { get; } = $"{Repo}/issues";
    }
}