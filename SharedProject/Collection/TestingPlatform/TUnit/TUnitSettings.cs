namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    public sealed class TUnitSettings
    {
        public TUnitSettings(
            string exePath,
            string settingsPath,
            string outputPath,
            string additionalArgs)
        {
            ExePath = exePath;
            SettingsPath = settingsPath;
            OutputPath = outputPath;
            AdditionalArgs = additionalArgs;
        }

        public string ExePath { get; }

        public string SettingsPath { get; }

        public string OutputPath { get; }

        public string AdditionalArgs { get; }
    }
}
