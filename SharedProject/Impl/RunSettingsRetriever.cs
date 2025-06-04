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
            this._userSettings = userSettings;

            string runSettingsFile = this.GetDefaultRunSettingsFilePath();
            string projectRunSettingsFile = await projectData.GetBuildPropertyAsync("RunSettingsFilePath", (string)null);

            return !string.IsNullOrEmpty(projectRunSettingsFile) ? projectRunSettingsFile : runSettingsFile;
        }

        private string GetAndUpdateSolutionRunSettingsFilePath()
            => this._userSettings.GetType()
                .GetMethod(
                    "GetAndUpdateSolutionRunSettingsFilePath",
                    BindingFlags.Public | BindingFlags.Instance
                ).Invoke(this._userSettings, new object[] { }) as string;

        private string LastRunSettingsFilePath()
            => this._userSettings.GetType()
                .GetProperty(
                    "LastRunSettingsFilePath",
                    BindingFlags.Public | BindingFlags.Instance
                ).GetValue(this._userSettings) as string;

        private bool AutomaticallyDetectRunSettings()
            => (bool)this._userSettings.GetType()
            .GetProperty("AutomaticallyDetectRunSettings", BindingFlags.Public | BindingFlags.Instance)
            .GetValue(this._userSettings);

        private string GetDefaultRunSettingsFilePath()
        {
            string settingsFilePath = this.GetAndUpdateSolutionRunSettingsFilePath();
            string lastRunSettingsFilePath = this.LastRunSettingsFilePath();

            return !string.IsNullOrEmpty(lastRunSettingsFilePath)
                ? lastRunSettingsFilePath
                : !this.AutomaticallyDetectRunSettings() || string.IsNullOrEmpty(settingsFilePath) ? null : settingsFilePath;
        }
    }
}
