using FineCodeCoverage.Options.IncludesExcludes;
using FineCodeCoverage.Options.Run;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    public sealed class CoverageSettings :
        ICoverageSettings,

        IFCCCommonIncludesExcludes,
        IMsCodeCoverageIncludesExcludesOptions,
        IEnabledOption
    {
        #region IFCCCommonIncludesExcludes
        public bool IncludeTestAssembly { get; set; }

        public bool IncludeReferencedProjects { get; set; }

        public string[] ExcludeAssemblies { get; set; }

        public string[] IncludeAssemblies { get; set; }

        #endregion
        #region IEnabledOption
        public bool Enabled { get; set; }

        #endregion
        #region IMsCodeCoverageIncludesExcludesOptions
        public string[] ModulePathsInclude { get; set; }

        public string[] ModulePathsExclude { get; set; }

        public string[] CompanyNamesExclude { get; set; }

        public string[] CompanyNamesInclude { get; set; }

        public string[] PublicKeyTokensExclude { get; set; }

        public string[] PublicKeyTokensInclude { get; set; }

        public string[] SourcesExclude { get; set; }

        public string[] SourcesInclude { get; set; }

        public string[] AttributesExclude { get; set; }

        public string[] AttributesInclude { get; set; }

        public string[] FunctionsInclude { get; set; }

        public string[] FunctionsExclude { get; set; }
        #endregion
    }
}
