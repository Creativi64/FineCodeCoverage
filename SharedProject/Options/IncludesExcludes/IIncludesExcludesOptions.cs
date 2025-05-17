namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal interface IFCCCommonIncludesExcludes
    {

        bool IncludeTestAssembly { get; set; }
        bool IncludeReferencedProjects { get; set; }

        string[] ExcludeAssemblies { get; set; }
        string[] IncludeAssemblies { get; set; }
    }

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

    internal interface IOpenCoverCoverletExcludeIncludeOptions
    {
        string[] Exclude { get; set; }
        string[] ExcludeByAttribute { get; set; }
        string[] ExcludeByFile { get; set; }
        string[] Include { get; set; }
    }



    internal interface IIncludesExcludesOptions :
        IMsCodeCoverageIncludesExcludesOptions,
        IOpenCoverCoverletExcludeIncludeOptions,
        IFCCCommonIncludesExcludes
    { }
}