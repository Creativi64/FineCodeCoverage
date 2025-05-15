using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoverageSettings
    {
        #region ICoverletOptions
        string CoverletConsoleCustomPath { get; }
        bool CoverletConsoleGlobal { get; }
        bool CoverletConsoleLocal { get; }
        bool RunSettingsOnly { get; }
        string CoverletCollectorDirectoryPath { get; }
        #endregion
        #region IOpenCoverOptions
        string OpenCoverTarget { get; }
        string OpenCoverTargetArgs { get; }
        OpenCoverRegister OpenCoverRegister { get; }
        string OpenCoverCustomPath { get; }
        #endregion
        #region IEnabledOption
        bool Enabled { get; }
        #endregion
        #region IFCCCommonIncludesExcludes
        bool IncludeTestAssembly { get; set; }
        bool IncludeReferencedProjects { get; }
        string[] ExcludeAssemblies { get; }
        string[] IncludeAssemblies { get; }
        #endregion
        #region IOpenCoverCoverletExcludeIncludeOptions
        string[] Exclude { get; }
        string[] Include { get; }

        string[] ExcludeByFile { get; set; }
        string[] ExcludeByAttribute { get; }
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
