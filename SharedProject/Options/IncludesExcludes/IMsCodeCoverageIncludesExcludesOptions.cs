namespace FineCodeCoverage.Options
{
    internal interface IMsCodeCoverageIncludesExcludesOptions
    {
        string[] ModulePathsExclude { get; set; }
        string[] ModulePathsInclude { get; set; }
        string[] CompanyNamesExclude { get; set; }
        string[] CompanyNamesInclude { get; set; }
        string[] PublicKeyTokensExclude { get; set; }
        string[] PublicKeyTokensInclude { get; set; }
        string[] SourcesExclude { get; set; }
        string[] SourcesInclude { get; set; }
        string[] AttributesExclude { get; set; }
        string[] AttributesInclude { get; set; }
        string[] FunctionsInclude { get; set; }
        string[] FunctionsExclude { get; set; }
    }
}
