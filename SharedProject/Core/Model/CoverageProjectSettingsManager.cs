using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICoverageProjectSettingsManager))]
    internal class CoverageProjectSettingsManager : ICoverageProjectSettingsManager
    {
        private readonly ICoverageSettingsOptionsProvider coveragSettingsOptionsProvider;
        private readonly ICoverageProjectSettingsProvider coverageProjectSettingsProvider;
        private readonly IFCCSettingsFilesProvider fccSettingsFilesProvider;
        private readonly ISettingsMerger settingsMerger;
        private readonly ICoverageSettingsReflectionService coverageSettingsReflectionService;

        [ImportingConstructor]
        public CoverageProjectSettingsManager(
            ICoverageSettingsOptionsProvider coveragSettingsOptionsProvider,
            ICoverageProjectSettingsProvider coverageProjectSettingsProvider,
            IFCCSettingsFilesProvider fccSettingsFilesProvider,
            ISettingsMerger settingsMerger,
            ICoverageSettingsReflectionService coverageSettingsReflectionService
        )
        {
            this.coveragSettingsOptionsProvider = coveragSettingsOptionsProvider;
            this.coverageProjectSettingsProvider = coverageProjectSettingsProvider;
            this.fccSettingsFilesProvider = fccSettingsFilesProvider;
            this.settingsMerger = settingsMerger;
            this.coverageSettingsReflectionService = coverageSettingsReflectionService;
        }

        private CoverageSettings GetSettingsFromAppOptions()
            => this.coverageSettingsReflectionService.CreateCoverageSettingsFromOptions(
                this.coveragSettingsOptionsProvider.Get());

        public async Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject)
        {
            var settingsFilesElements = GetSettingsFilesElements(coverageProject);
            var projectSettingsElement = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            var coverageSettings = GetSettingsFromAppOptions();
            if (settingsFilesElements.Count > 0 || projectSettingsElement != null)
            {
                await settingsMerger.MergeAsync(
                    coverageSettings, coverageSettingsReflectionService.CoverageSettingsPropertyInfos, settingsFilesElements, projectSettingsElement
                );
            }

            AddCommonAssemblyExcludesIncludes(coverageSettings);
            return coverageSettings;
        }

        private List<XElement> GetSettingsFilesElements(ICoverageProject coverageProject)
        {
            var projectDirectory = Path.GetDirectoryName(coverageProject.ProjectFilePath);
            return fccSettingsFilesProvider.Provide(projectDirectory);
        }

        private void AddCommonAssemblyExcludesIncludes(CoverageSettings coverageSettings)
        {
            var (newOldStyleExclude,newMsExclude) = AddCommon(
                coverageSettings.Exclude, coverageSettings.ModulePathsExclude, coverageSettings.ExcludeAssemblies);
            var (newOldStyleInclude,newMsInclude) = AddCommon(
                coverageSettings.Include, coverageSettings.ModulePathsInclude, coverageSettings.IncludeAssemblies);
            coverageSettings.Exclude = newOldStyleExclude;
            coverageSettings.Include = newOldStyleInclude;
            coverageSettings.ModulePathsExclude = newMsExclude;
            coverageSettings.ModulePathsInclude = newMsInclude;
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
