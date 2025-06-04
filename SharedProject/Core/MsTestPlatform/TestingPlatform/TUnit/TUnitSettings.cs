namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class TUnitSettings
    {
        public TUnitSettings(
            string exePath,
            string settingsPath,
            string outputPath,
            string additionalArgs
            )
        {
            this.ExePath = exePath;
            this.SettingsPath = settingsPath;
            this.OutputPath = outputPath;
            this.AdditionalArgs = additionalArgs;
        }

        public string ExePath { get; }
        public string SettingsPath { get; }
        public string OutputPath { get; }
        public string AdditionalArgs { get; }
    }
}
