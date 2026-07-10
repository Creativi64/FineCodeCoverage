namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    public interface ICoverageSettings
    {
        #region IEnabledOption
        bool Enabled { get; }

        #endregion
        #region IFCCCommonIncludesExcludes
        bool IncludeTestAssembly { get; set; }

        bool IncludeReferencedProjects { get; }

        string[] ExcludeAssemblies { get; }

        string[] IncludeAssemblies { get; }

        #endregion
        #region IMsCodeCoverageIncludesExcludesOptions

        string[] ModulePathsExclude { get; }

        string[] ModulePathsInclude { get; }

        string[] CompanyNamesExclude { get; }

        string[] CompanyNamesInclude { get; }

        string[] PublicKeyTokensExclude { get; }

        string[] PublicKeyTokensInclude { get; }

        string[] SourcesExclude { get; }

        string[] SourcesInclude { get; }

        string[] AttributesExclude { get; }

        string[] AttributesInclude { get; }

        string[] FunctionsInclude { get; }

        string[] FunctionsExclude { get; }
        #endregion
    }
}
