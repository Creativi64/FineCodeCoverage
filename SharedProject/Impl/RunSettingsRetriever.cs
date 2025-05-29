using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(IRunSettingsRetriever))]
    internal class RunSettingsRetriever : IRunSettingsRetriever
    {
        private object userSettings;

        public async Task<string> GetRunSettingsFileAsync(object userSettings, ContainerData projectData)
        {
            this.userSettings = userSettings;

            string runSettingsFile = this.GetDefaultRunSettingsFilePath();
            string projectRunSettingsFile = await projectData.GetBuildPropertyAsync("RunSettingsFilePath", (string)null);

            if (!string.IsNullOrEmpty(projectRunSettingsFile))
            {
                return projectRunSettingsFile;
            }

            return runSettingsFile;
        }

        private string GetAndUpdateSolutionRunSettingsFilePath()
        {
            return this.userSettings.GetType().GetMethod("GetAndUpdateSolutionRunSettingsFilePath", BindingFlags.Public | BindingFlags.Instance).Invoke(this.userSettings, new object[] { }) as string;
        }

        private string LastRunSettingsFilePath()
        {
            return this.userSettings.GetType().GetProperty("LastRunSettingsFilePath", BindingFlags.Public | BindingFlags.Instance).GetValue(this.userSettings) as string;
        }

        private bool AutomaticallyDetectRunSettings()
        {
            return (bool)this.userSettings.GetType().GetProperty("AutomaticallyDetectRunSettings", BindingFlags.Public | BindingFlags.Instance).GetValue(this.userSettings);
        }

        private string GetDefaultRunSettingsFilePath()
        {
            string settingsFilePath = this.GetAndUpdateSolutionRunSettingsFilePath();
            string lastRunSettingsFilePath = this.LastRunSettingsFilePath();

            if (!string.IsNullOrEmpty(lastRunSettingsFilePath))
            {
                return lastRunSettingsFilePath;
            }

            if (!this.AutomaticallyDetectRunSettings() || string.IsNullOrEmpty(settingsFilePath))
            {
                return null;
            }

            return settingsFilePath;
        }

    }
}
