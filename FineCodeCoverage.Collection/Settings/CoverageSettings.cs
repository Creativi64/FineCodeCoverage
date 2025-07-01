using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.Model
{
    public sealed class CoverageSettings :
        ICoverageSettings,

        ICoverletOptions,
        IOpenCoverOptions,
        IFCCCommonIncludesExcludes,
        IOpenCoverCoverletExcludeIncludeOptions,
        IMsCodeCoverageIncludesExcludesOptions,
        IEnabledOption
    {
        #region ICoverletOptions
        public string CoverletConsoleCustomPath { get; set; }

        public bool CoverletConsoleGlobal { get; set; }

        public bool CoverletConsoleLocal { get; set; }

        public string CoverletCollectorDirectoryPath { get; set; }

        public bool RunSettingsOnly { get; set; }

        #endregion
        #region IOpenCoverOptions
        public string OpenCoverTarget { get; set; }

        public string OpenCoverTargetArgs { get; set; }

        public OpenCoverRegister OpenCoverRegister { get; set; }

        public string OpenCoverCustomPath { get; set; }

        #endregion
        #region IFCCCommonIncludesExcludes
        public bool IncludeTestAssembly { get; set; }

        public bool IncludeReferencedProjects { get; set; }

        public string[] ExcludeAssemblies { get; set; }

        public string[] IncludeAssemblies { get; set; }

        #endregion
        #region IOpenCoverCoverletExcludeIncludeOptions
        public string[] Exclude { get; set; }

        public string[] Include { get; set; }

        public string[] ExcludeByFile { get; set; }

        public string[] ExcludeByAttribute { get; set; }

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
