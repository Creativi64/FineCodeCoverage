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
        private readonly IFileUtil fileUtil;
        private readonly IVsRunSettingsWriter vsRunSettingsWriter;
        private readonly ILogger logger;
        private const string fccGeneratedRunSettingsSuffix = "fcc-mscodecoverage-generated";

        [ImportingConstructor]
        public ProjectRunSettingsGenerator(
            IFileUtil fileUtil,
            IVsRunSettingsWriter vsRunSettingsWriter,
            ILogger logger
        )
        {
            this.fileUtil = fileUtil;
            this.vsRunSettingsWriter = vsRunSettingsWriter;
            this.logger = logger;
        }

        public Task RemoveGeneratedProjectSettingsAsync(IEnumerable<ICoverageProject> coverageProjects)
            => Task.WhenAll(
                coverageProjects
                .Where(coverageProject => IsGeneratedRunSettings(coverageProject.RunSettingsFile))
                .Select(coverageProjectForRemoval => this.vsRunSettingsWriter.RemoveRunSettingsFilePathAsync(coverageProjectForRemoval.Id))
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
                $"{coverageProject.ProjectName}-{fccGeneratedRunSettingsSuffix}.runsettings");

        private async Task WriteProjectRunSettingsAsync(Guid projectGuid, string projectName, string projectRunSettingsFilePath, string projectRunSettings)
        {
            bool ok = await this.vsRunSettingsWriter.WriteRunSettingsFilePathAsync(projectGuid, projectRunSettingsFilePath);
            if (ok)
            {
                projectRunSettings = XDocument.Parse(projectRunSettings).FormatXml();
                this.fileUtil.WriteAllText(projectRunSettingsFilePath, projectRunSettings);
                await this.logger.LogAsync($"runsettings written to {projectRunSettingsFilePath}", projectRunSettings);
            }
            else
            {
                await this.logger.LogAsync($"Issue writing runsettings for {projectName}");
            }
        }

        private static bool IsGeneratedRunSettings(string runSettingsFile)
            => runSettingsFile != null && Path.GetFileNameWithoutExtension(runSettingsFile).EndsWith(fccGeneratedRunSettingsSuffix);
    }
}