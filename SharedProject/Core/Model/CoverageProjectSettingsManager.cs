using FineCodeCoverage.Options;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Model
{

    internal class CoverageSettings : ICoverageSettings
    {
        public bool IncludeReferencedProjects { get; }
        public string CoverletConsoleCustomPath { get; }
        public bool CoverletConsoleGlobal { get; }
        public bool CoverletConsoleLocal { get; }
        public string[] Exclude { get; set; }
        public string[] Include { get; set; }
        public bool IncludeTestAssembly { get; set; }
        public string[] ExcludeByFile { get; set; }
        public string[] ExcludeByAttribute { get; }
        public bool RunSettingsOnly { get; }
        public string CoverletCollectorDirectoryPath { get; }
        public bool Enabled { get; }
        public string OpenCoverTarget { get; }
        public string OpenCoverTargetArgs { get; }
        public OpenCoverRegister OpenCoverRegister { get; }
        public string[] ModulePathsInclude { get; set; }
        public string OpenCoverCustomPath { get; }
        public string[] ModulePathsExclude { get; set; }
        public string[] ExcludeAssemblies { get; }
        public string[] IncludeAssemblies { get; }
    }

    [Export(typeof(ICoverageProjectSettingsManager))]
    internal class CoverageProjectSettingsManager : ICoverageProjectSettingsManager
    {
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly ICoverageProjectSettingsProvider coverageProjectSettingsProvider;
        private readonly IFCCSettingsFilesProvider fccSettingsFilesProvider;
        private readonly ISettingsMerger settingsMerger;

        [ImportingConstructor]
        public CoverageProjectSettingsManager(
            IAppOptionsProvider appOptionsProvider,
            ICoverageProjectSettingsProvider coverageProjectSettingsProvider,
            IFCCSettingsFilesProvider fccSettingsFilesProvider,
            ISettingsMerger settingsMerger
        )
        {
            this.appOptionsProvider = appOptionsProvider;
            this.coverageProjectSettingsProvider = coverageProjectSettingsProvider;
            this.fccSettingsFilesProvider = fccSettingsFilesProvider;
            this.settingsMerger = settingsMerger;
        }

        public async Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject)
        {
            throw new System.NotImplementedException();
            //var projectDirectory = Path.GetDirectoryName(coverageProject.ProjectFile);
            //var settingsFilesElements = fccSettingsFilesProvider.Provide(projectDirectory);
            //var projectSettingsElement = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            //var merged = await settingsMerger.MergeAsync(appOptionsProvider.Get(), settingsFilesElements, projectSettingsElement);
            //AddCommonAssemblyExcludesIncludes(merged);
            //return merged;
        }

        private void AddCommonAssemblyExcludesIncludes(CoverageSettings appOptions)
        {
            var (newOldStyleExclude,newMsExclude) = AddCommon(appOptions.Exclude, appOptions.ModulePathsExclude, appOptions.ExcludeAssemblies);
            var (newOldStyleInclude,newMsInclude) = AddCommon(appOptions.Include, appOptions.ModulePathsInclude, appOptions.IncludeAssemblies);
            appOptions.Exclude = newOldStyleExclude;
            appOptions.Include = newOldStyleInclude;
            appOptions.ModulePathsExclude = newMsExclude;
            appOptions.ModulePathsInclude = newMsInclude;
        }

        private (string[] newOldStyle,string[] newMs) AddCommon(string[] oldStyle,string[] ms, string[] common )
        {
            if(common == null)
            {
                return(oldStyle,ms);
            }
            var newMs = ListFromExisting(ms);
            var newOldStyle = ListFromExisting(oldStyle);

            var nonWhitespaceCommon = common.Where(c => !string.IsNullOrWhiteSpace(c));
            foreach(var assemblyFileName in nonWhitespaceCommon)
            {
                var msModulePath = $".*\\{assemblyFileName}.dll$";
                newMs.Add(msModulePath);
                var old = $"[{assemblyFileName}]*";
                newOldStyle.Add(old);
            }

            return (newOldStyle.ToArray(), newMs.ToArray());
        }

        private List<string> ListFromExisting(string[] existing)
        {
            return new List<string>(existing ?? new string[0]);
        }
    }

}
