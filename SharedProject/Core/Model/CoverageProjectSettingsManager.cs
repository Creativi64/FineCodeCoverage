using System;
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
        private readonly ICoverageSettingsOptionsProvider _coveragSettingsOptionsProvider;
        private readonly ICoverageProjectSettingsProvider _coverageProjectSettingsProvider;
        private readonly IFCCSettingsFilesProvider _fccSettingsFilesProvider;
        private readonly ISettingsMerger _settingsMerger;
        private readonly ICoverageSettingsReflectionService _coverageSettingsReflectionService;

        [ImportingConstructor]
        public CoverageProjectSettingsManager(
            ICoverageSettingsOptionsProvider coveragSettingsOptionsProvider,
            ICoverageProjectSettingsProvider coverageProjectSettingsProvider,
            IFCCSettingsFilesProvider fccSettingsFilesProvider,
            ISettingsMerger settingsMerger,
            ICoverageSettingsReflectionService coverageSettingsReflectionService
        )
        {
            this._coveragSettingsOptionsProvider = coveragSettingsOptionsProvider;
            this._coverageProjectSettingsProvider = coverageProjectSettingsProvider;
            this._fccSettingsFilesProvider = fccSettingsFilesProvider;
            this._settingsMerger = settingsMerger;
            this._coverageSettingsReflectionService = coverageSettingsReflectionService;
        }

        private CoverageSettings GetSettingsFromOptions()
            => this._coverageSettingsReflectionService.CreateCoverageSettingsFromOptions(
                this._coveragSettingsOptionsProvider.Get());

        public async Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject)
        {
            List<XElement> settingsFilesElements = this.GetSettingsFilesElements(coverageProject);
            XElement projectSettingsElement = await this._coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            CoverageSettings coverageSettings = this.GetSettingsFromOptions();
            if (settingsFilesElements.Count > 0 || projectSettingsElement != null)
            {
                await this._settingsMerger.MergeAsync(
                    coverageSettings, this._coverageSettingsReflectionService.CoverageSettingsPropertyInfos, settingsFilesElements, projectSettingsElement
                );
            }

            AddCommonAssemblyExcludesIncludes(coverageSettings);
            return coverageSettings;
        }

        private List<XElement> GetSettingsFilesElements(ICoverageProject coverageProject)
        {
            string projectDirectory = Path.GetDirectoryName(coverageProject.ProjectFilePath);
            return this._fccSettingsFilesProvider.Provide(projectDirectory);
        }

        private static void AddCommonAssemblyExcludesIncludes(CoverageSettings coverageSettings)
        {
            (string[] newOldStyleExclude, string[] newMsExclude) = AddCommon(
                coverageSettings.Exclude, coverageSettings.ModulePathsExclude, coverageSettings.ExcludeAssemblies);
            (string[] newOldStyleInclude, string[] newMsInclude) = AddCommon(
                coverageSettings.Include, coverageSettings.ModulePathsInclude, coverageSettings.IncludeAssemblies);
            coverageSettings.Exclude = newOldStyleExclude;
            coverageSettings.Include = newOldStyleInclude;
            coverageSettings.ModulePathsExclude = newMsExclude;
            coverageSettings.ModulePathsInclude = newMsInclude;
        }

        private static (string[] newOldStyle, string[] newMs) AddCommon(string[] oldStyle, string[] ms, string[] common)
        {
            if (common == null)
            {
                return (oldStyle, ms);
            }

            List<string> newMs = ListFromExisting(ms);
            List<string> newOldStyle = ListFromExisting(oldStyle);

            IEnumerable<string> nonWhitespaceCommon = common.Where(c => !string.IsNullOrWhiteSpace(c));
            foreach (string assemblyFileName in nonWhitespaceCommon)
            {
                string msModulePath = $".*\\{assemblyFileName}.dll$";
                newMs.Add(msModulePath);
                string old = $"[{assemblyFileName}]*";
                newOldStyle.Add(old);
            }

            return (newOldStyle.ToArray(), newMs.ToArray());
        }

        private static List<string> ListFromExisting(string[] existing)
            => new List<string>(existing ?? Array.Empty<string>());
    }
}
