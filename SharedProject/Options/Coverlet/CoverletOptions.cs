using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
        Interfaces to be retained for reflection - AppOptions => CoverageSettings
    */
    internal class CoverletOptions : ICoverletOptions
    {
        private const string consoleCategory = "Console";
        private const string collectorCategory = "Collector";

        #region console category
        [Category(consoleCategory)]
        [Description("Specify true to use your own dotnet tools global install of coverlet console.")]
        [DisplayName("Coverlet Console Global")]
        public bool CoverletConsoleGlobal { get; set; }

        [Category(consoleCategory)]
        [Description("Specify true to use your own dotnet tools local install of coverlet console.")]
        [DisplayName("Coverlet Console Local")]
        public bool CoverletConsoleLocal { get; set; }

        [Category(consoleCategory)]
        [Description("Specify path to coverlet console exe if you need functionality that the FCC version does not provide.")]
        [DisplayName("Coverlet Console Custom Path")]
        public string CoverletConsoleCustomPath { get; set; }


        #endregion

        #region collector category
        [Category(collectorCategory)]
        [Description("Specify path to directory containing coverlet collector files if you need functionality that the FCC version does not provide.")]
        [DisplayName("Coverlet Collector Directory Path")]
        public string CoverletCollectorDirectoryPath { get; set; }
        [Description("Specify false for global and project options to be used for coverlet data collector configuration elements when not specified in runsettings")]
        [Category(collectorCategory)]
        [DisplayName("Run Settings Only")]
        public bool RunSettingsOnly { get; set; }
        #endregion
    }
}
