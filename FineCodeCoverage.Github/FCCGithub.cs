namespace FineCodeCoverage.Github
{
    public static class FCCGithub
    {
        public const string Repo = "https://github.com/FortuneN/FineCodeCoverage";

        public static string MasterBlob { get; } = $"{Repo}/blob/master/";

        public static string Readme { get; } = $"{MasterBlob}/README.md";

        public static string Issues { get; } = $"{Repo}/issues";
    }
}
