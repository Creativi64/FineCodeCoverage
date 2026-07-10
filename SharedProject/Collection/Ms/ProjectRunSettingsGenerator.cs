using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.Ms
{
    [Export(typeof(IProjectRunSettingsGenerator))]
    internal sealed class ProjectRunSettingsGenerator : IProjectRunSettingsGenerator
    {
        private const string FCCGeneratedRunSettingsSuffix = "fcc-mscodecoverage-generated";
        private readonly IVsRunSettingsWriter _vsRunSettingsWriter;

        [ImportingConstructor]
        public ProjectRunSettingsGenerator(IVsRunSettingsWriter vsRunSettingsWriter)
            => _vsRunSettingsWriter = vsRunSettingsWriter;

        // Migration only: removes any RunSettingsFilePath that an earlier FCC build wrote into a project
        // file.  Coverage runsettings are now injected in-memory (see UserRunSettingsService.AddFCCRunSettings),
        // so FCC no longer writes them to disk.
        public Task RemoveGeneratedProjectSettingsAsync(IEnumerable<ICoverageProject> coverageProjects)
            => Task.WhenAll(
                coverageProjects
                .Where(coverageProject => IsGeneratedRunSettings(coverageProject.RunSettingsFile))
                .Select(coverageProjectForRemoval => _vsRunSettingsWriter.RemoveRunSettingsFilePathAsync(coverageProjectForRemoval.Id)));

        private static bool IsGeneratedRunSettings(string runSettingsFile)
            => runSettingsFile != null && Path.GetFileNameWithoutExtension(runSettingsFile).EndsWith(FCCGeneratedRunSettingsSuffix);
    }
}
