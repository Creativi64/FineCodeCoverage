namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    public sealed class TUnitSettings
    {
        /// <summary>
        /// The only output format FCC asks for when the user has not chosen one.
        /// </summary>
        public const string DefaultOutputFormat = "cobertura";

        public TUnitSettings(
            string exePath,
            string settingsPath,
            bool userSuppliedSettingsPath,
            string fccOutputPath,
            string userOutputPath,
            string userOutputFormat,
            string additionalArgs)
        {
            ExePath = exePath;
            SettingsPath = settingsPath;
            UserSuppliedSettingsPath = userSuppliedSettingsPath;
            FccOutputPath = fccOutputPath;
            UserOutputPath = userOutputPath;
            UserOutputFormat = userOutputFormat;
            AdditionalArgs = additionalArgs;
        }

        public string ExePath { get; }

        /// <summary>
        /// The code coverage Configuration xml to use - either the user's own file or the one FCC generated.
        /// </summary>
        public string SettingsPath { get; }

        /// <summary>
        /// True when <see cref="SettingsPath"/> came from the user's --coverage-settings / --settings argument.
        /// </summary>
        public bool UserSuppliedSettingsPath { get; }

        /// <summary>
        /// Where FCC would write coverage if the user did not ask for somewhere else.
        /// </summary>
        public string FccOutputPath { get; }

        /// <summary>
        /// The user's --coverage-output argument, null when they did not supply one.
        /// </summary>
        public string UserOutputPath { get; }

        /// <summary>
        /// The user's --coverage-output-format argument, null when they did not supply one
        /// or supplied one that ReportGenerator cannot read.
        /// </summary>
        public string UserOutputFormat { get; }

        public string OutputPath => UserOutputPath ?? FccOutputPath;

        public string OutputFormat => UserOutputFormat ?? DefaultOutputFormat;

        public string AdditionalArgs { get; }
    }
}
