using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IProjectRunSettingsGenerator))]
    internal class ProjectRunSettingsGenerator : IProjectRunSettingsGenerator
    {
        private readonly IFileUtil _fileUtil;
        private readonly IVsRunSettingsWriter _vsRunSettingsWriter;
        private readonly ILogger _logger;
        private const string FCCGeneratedRunSettingsSuffix = "fcc-mscodecoverage-generated";

        [ImportingConstructor]
        public ProjectRunSettingsGenerator(
            IFileUtil fileUtil,
            IVsRunSettingsWriter vsRunSettingsWriter,
            ILogger logger
        )
        {
            this._fileUtil = fileUtil;
            this._vsRunSettingsWriter = vsRunSettingsWriter;
            this._logger = logger;
        }

        public Task RemoveGeneratedProjectSettingsAsync(IEnumerable<ICoverageProject> coverageProjects)
            => Task.WhenAll(
                coverageProjects
                .Where(coverageProject => IsGeneratedRunSettings(coverageProject.RunSettingsFile))
                .Select(coverageProjectForRemoval => this._vsRunSettingsWriter.RemoveRunSettingsFilePathAsync(coverageProjectForRemoval.Id))
            );

        public Task WriteProjectsRunSettingsAsync(IEnumerable<ICoverageProjectRunSettings> coverageProjectsRunSettings)
            => Task.WhenAll(
                coverageProjectsRunSettings.Select(coverageProjectRunSettings =>
                {
                    ICoverageProject coverageProject = coverageProjectRunSettings.CoverageProject;
                    string projectRunSettingsFilePath = GeneratedProjectRunSettingsFilePath(coverageProject);
                    return this.WriteProjectRunSettingsAsync(coverageProject.Id, coverageProject.ProjectName, projectRunSettingsFilePath, coverageProjectRunSettings.RunSettings);
                })
            );

        internal static string GeneratedProjectRunSettingsFilePath(ICoverageProject coverageProject)
            => Path.Combine(
                coverageProject.CoverageOutputFolder,
                $"{coverageProject.ProjectName}-{FCCGeneratedRunSettingsSuffix}.runsettings");

        private async Task WriteProjectRunSettingsAsync(Guid projectGuid, string projectName, string projectRunSettingsFilePath, string projectRunSettings)
        {
            bool ok = await this._vsRunSettingsWriter.WriteRunSettingsFilePathAsync(projectGuid, projectRunSettingsFilePath);
            if (ok)
            {
                projectRunSettings = XDocument.Parse(projectRunSettings).FormatXml();
                this._fileUtil.WriteAllText(projectRunSettingsFilePath, projectRunSettings);
                await this._logger.LogAsync($"runsettings written to {projectRunSettingsFilePath}", projectRunSettings);
            }
            else
            {
                await this._logger.LogAsync($"Issue writing runsettings for {projectName}");
            }
        }

        private static bool IsGeneratedRunSettings(string runSettingsFile)
            => runSettingsFile != null && Path.GetFileNameWithoutExtension(runSettingsFile).EndsWith(FCCGeneratedRunSettingsSuffix);
    }
}