using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IProjectRunSettingsGenerator))]
    internal sealed class ProjectRunSettingsGenerator : IProjectRunSettingsGenerator
    {
        private const string FCCGeneratedRunSettingsSuffix = "fcc-mscodecoverage-generated";
        private readonly IFileUtil _fileUtil;
        private readonly IVsRunSettingsWriter _vsRunSettingsWriter;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public ProjectRunSettingsGenerator(
            IFileUtil fileUtil,
            IVsRunSettingsWriter vsRunSettingsWriter,
            ILogger logger)
        {
            _fileUtil = fileUtil;
            _vsRunSettingsWriter = vsRunSettingsWriter;
            _logger = logger;
        }

        public Task RemoveGeneratedProjectSettingsAsync(IEnumerable<ICoverageProject> coverageProjects)
            => Task.WhenAll(
                coverageProjects
                .Where(coverageProject => IsGeneratedRunSettings(coverageProject.RunSettingsFile))
                .Select(coverageProjectForRemoval => _vsRunSettingsWriter.RemoveRunSettingsFilePathAsync(coverageProjectForRemoval.Id)));

        public Task WriteProjectsRunSettingsAsync(IEnumerable<ICoverageProjectRunSettings> coverageProjectsRunSettings)
            => Task.WhenAll(
                coverageProjectsRunSettings.Select(coverageProjectRunSettings =>
                {
                    ICoverageProject coverageProject = coverageProjectRunSettings.CoverageProject;
                    string projectRunSettingsFilePath = GeneratedProjectRunSettingsFilePath(coverageProject);
                    return WriteProjectRunSettingsAsync(coverageProject.Id, coverageProject.ProjectName, projectRunSettingsFilePath, coverageProjectRunSettings.RunSettings);
                }));

        internal static string GeneratedProjectRunSettingsFilePath(ICoverageProject coverageProject)
            => Path.Combine(
                coverageProject.CoverageOutputFolder,
                $"{coverageProject.ProjectName}-{FCCGeneratedRunSettingsSuffix}.runsettings");

        private async Task WriteProjectRunSettingsAsync(Guid projectGuid, string projectName, string projectRunSettingsFilePath, string projectRunSettings)
        {
            bool ok = await _vsRunSettingsWriter.WriteRunSettingsFilePathAsync(projectGuid, projectRunSettingsFilePath);
            if (ok)
            {
                projectRunSettings = XDocument.Parse(projectRunSettings).FormatXml();
                _fileUtil.WriteAllText(projectRunSettingsFilePath, projectRunSettings);
                await _logger.LogAsync($"runsettings written to {projectRunSettingsFilePath}", projectRunSettings);
            }
            else
            {
                await _logger.LogAsync($"Issue writing runsettings for {projectName}");
            }
        }

        private static bool IsGeneratedRunSettings(string runSettingsFile)
            => runSettingsFile != null && Path.GetFileNameWithoutExtension(runSettingsFile).EndsWith(FCCGeneratedRunSettingsSuffix);
    }
}
