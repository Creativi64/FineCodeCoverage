using System.Collections.Generic;

namespace FineCodeCoverage.ReportGeneration
{
    internal class FCCReportConfiguration
    {
        public FCCReportConfiguration(
            IEnumerable<string> coverageOutputFiles,
            string targetDirectory,
            IEnumerable<string> sourceDirectories,
            string historyDirectory,
            IEnumerable<string> reportTypes,
            IEnumerable<string> plugins,
            IEnumerable<string> assemblyFilters,
            IEnumerable<string> classFilters,
            IEnumerable<string> fileFilters,
            string verbosityLevel,
            string tag
        )
        {
            this.CoverageOutputFiles = coverageOutputFiles;
            this.TargetDirectory = targetDirectory;
            this.SourceDirectories = sourceDirectories;
            this.HistoryDirectory = historyDirectory;
            this.ReportTypes = reportTypes;
            this.Plugins = plugins;
            this.AssemblyFilters = assemblyFilters;
            this.ClassFilters = classFilters;
            this.FileFilters = fileFilters;
            this.VerbosityLevel = verbosityLevel;
            this.Tag = tag;
        }

        public IEnumerable<string> CoverageOutputFiles { get; }
        public string TargetDirectory { get; }
        public IEnumerable<string> SourceDirectories { get; }
        public string HistoryDirectory { get; }
        public IEnumerable<string> ReportTypes { get; }
        public IEnumerable<string> Plugins { get; }
        public IEnumerable<string> AssemblyFilters { get; }
        public IEnumerable<string> ClassFilters { get; }
        public IEnumerable<string> FileFilters { get; }
        public string VerbosityLevel { get; }
        public string Tag { get; }
    }
}
