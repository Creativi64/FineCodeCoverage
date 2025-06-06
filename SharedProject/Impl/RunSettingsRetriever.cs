using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(IRunSettingsRetriever))]
    internal class RunSettingsRetriever : IRunSettingsRetriever
    {
        private object _userSettings;

        public async Task<string> GetRunSettingsFileAsync(object userSettings, ContainerData projectData)
        {
            _userSettings = userSettings;

            string runSettingsFile = GetDefaultRunSettingsFilePath();
            string projectRunSettingsFile = await projectData.GetBuildPropertyAsync("RunSettingsFilePath", null);

            return !string.IsNullOrEmpty(projectRunSettingsFile) ? projectRunSettingsFile : runSettingsFile;
        }

        private string GetAndUpdateSolutionRunSettingsFilePath()
            => _userSettings.GetType()
                .GetMethod(
                    "GetAndUpdateSolutionRunSettingsFilePath",
                    BindingFlags.Public | BindingFlags.Instance)
                .Invoke(_userSettings, new object[] { }) as string;

        private string LastRunSettingsFilePath()
            => _userSettings.GetType()
                .GetProperty(
                    "LastRunSettingsFilePath",
                    BindingFlags.Public | BindingFlags.Instance)
                .GetValue(_userSettings) as string;

        private bool AutomaticallyDetectRunSettings()
            => (bool)_userSettings.GetType()
            .GetProperty("AutomaticallyDetectRunSettings", BindingFlags.Public | BindingFlags.Instance)
            .GetValue(_userSettings);

        private string GetDefaultRunSettingsFilePath()
        {
            string settingsFilePath = GetAndUpdateSolutionRunSettingsFilePath();
            string lastRunSettingsFilePath = LastRunSettingsFilePath();

            return !string.IsNullOrEmpty(lastRunSettingsFilePath)
                ? lastRunSettingsFilePath
                : !AutomaticallyDetectRunSettings() || string.IsNullOrEmpty(settingsFilePath) ? null : settingsFilePath;
        }
    }
}
