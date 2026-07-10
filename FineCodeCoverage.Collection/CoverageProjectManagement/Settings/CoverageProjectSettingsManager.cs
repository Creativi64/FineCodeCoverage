using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.VSAbstractions.Threading;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    [Export(typeof(ICoverageProjectSettingsManager))]
    internal sealed class CoverageProjectSettingsManager : ICoverageProjectSettingsManager
    {
        private readonly ICoverageSettingsOptionsProvider _coveragSettingsOptionsProvider;
        private readonly ICoverageProjectSettingsProvider _coverageProjectSettingsProvider;
        private readonly IFCCSettingsFilesProvider _fccSettingsFilesProvider;
        private readonly ISettingsMerger _settingsMerger;
        private readonly ICoverageSettingsReflectionService _coverageSettingsReflectionService;
        private readonly IThreadHelper _threadHelper;

        [ImportingConstructor]
        public CoverageProjectSettingsManager(
            ICoverageSettingsOptionsProvider coveragSettingsOptionsProvider,
            ICoverageProjectSettingsProvider coverageProjectSettingsProvider,
            IFCCSettingsFilesProvider fccSettingsFilesProvider,
            ISettingsMerger settingsMerger,
            ICoverageSettingsReflectionService coverageSettingsReflectionService,
            IThreadHelper threadHelper)
        {
            _coveragSettingsOptionsProvider = coveragSettingsOptionsProvider;
            _coverageProjectSettingsProvider = coverageProjectSettingsProvider;
            _fccSettingsFilesProvider = fccSettingsFilesProvider;
            _settingsMerger = settingsMerger;
            _coverageSettingsReflectionService = coverageSettingsReflectionService;
            _threadHelper = threadHelper;
        }

        private CoverageSettings GetSettingsFromOptions()
            => _coverageSettingsReflectionService.CreateCoverageSettingsFromOptions(
                _coveragSettingsOptionsProvider.Get());

        public async Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject)
        {
            List<XElement> settingsFilesElements = GetSettingsFilesElements(coverageProject);
            XElement projectSettingsElement = await _coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            CoverageSettings coverageSettings = GetSettingsFromOptions();
            if (settingsFilesElements.Count > 0 || projectSettingsElement != null)
            {
                await _settingsMerger.MergeAsync(
                    coverageSettings, _coverageSettingsReflectionService.CoverageSettingsPropertyInfos, settingsFilesElements, projectSettingsElement);
            }

            AddCommonAssemblyExcludesIncludes(coverageSettings);
            return coverageSettings;
        }

        private List<XElement> GetSettingsFilesElements(ICoverageProject coverageProject)
        {
            string projectDirectory = Path.GetDirectoryName(coverageProject.ProjectFilePath);
            return _fccSettingsFilesProvider.Provide(projectDirectory);
        }

        private static void AddCommonAssemblyExcludesIncludes(CoverageSettings coverageSettings)
        {
            coverageSettings.ModulePathsExclude = AddCommon(coverageSettings.ModulePathsExclude, coverageSettings.ExcludeAssemblies);
            coverageSettings.ModulePathsInclude = AddCommon(coverageSettings.ModulePathsInclude, coverageSettings.IncludeAssemblies);
        }

        private static string[] AddCommon(string[] ms, string[] common)
        {
            if (common == null)
            {
                return ms;
            }

            List<string> newMs = ListFromExisting(ms);

            IEnumerable<string> nonWhitespaceCommon = common.Where(c => !string.IsNullOrWhiteSpace(c));
            foreach (string assemblyFileName in nonWhitespaceCommon)
            {
                string msModulePath = $".*\\{assemblyFileName}.dll$";
                newMs.Add(msModulePath);
            }

            return newMs.ToArray();
        }

        private static List<string> ListFromExisting(string[] existing)
            => new List<string>(existing ?? Array.Empty<string>());

        public ICoverageSettings GetSettings(ICoverageProject coverageProject) => _threadHelper.JoinableTaskFactory.Run(
                       async () => await GetSettingsAsync(coverageProject));
    }
}
