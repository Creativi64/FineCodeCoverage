using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    internal interface ICoverletOptions
    {
        bool CoverletConsoleGlobal { get; set; }
        string CoverletConsoleCustomPath { get; set; }
        bool CoverletConsoleLocal { get; set; }
        string CoverletCollectorDirectoryPath { get; set; }
        bool RunSettingsOnly { get; set; }
    }
    /*
        Note that option properties must not be renamed
        Interfaces to be retained for reflection - AppOptions => CoverageSettings
    */
    internal class CoverletOptions : ICoverletOptions
    {
        private const string coverletInstallCategory = "Install";
        private const string coverletCollectorCategory = "Collector";
        #region install category
        [Category(coverletInstallCategory)]
        [Description("Specify true to use your own dotnet tools global install of coverlet console.")]
        [DisplayName("Coverlet Console Global")]
        public bool CoverletConsoleGlobal { get; set; }

        [Category(coverletInstallCategory)]
        [Description("Specify true to use your own dotnet tools local install of coverlet console.")]
        [DisplayName("Coverlet Console Local")]
        public bool CoverletConsoleLocal { get; set; }

        [Category(coverletInstallCategory)]
        [Description("Specify path to coverlet console exe if you need functionality that the FCC version does not provide.")]
        [DisplayName("Coverlet Console Custom Path")]
        public string CoverletConsoleCustomPath { get; set; }

        [Category(coverletInstallCategory)]
        [Description("Specify path to directory containing coverlet collector files if you need functionality that the FCC version does not provide.")]
        [DisplayName("Coverlet Collector Directory Path")]
        public string CoverletCollectorDirectoryPath { get; set; }
        #endregion

        #region collector category
        [Description("Specify false for global and project options to be used for coverlet data collector configuration elements when not specified in runsettings")]
        [Category(coverletCollectorCategory)]
        [DisplayName("Run Settings Only")]
        public bool RunSettingsOnly { get; set; }
        #endregion
    }
}
