namespace FineCodeCoverage.Core.Coverlet
{
    internal interface IRunSettingsCoverletConfiguration
    {
        CoverletDataCollectorState CoverletDataCollectorState { get; }

        string Format { get; }

        string Exclude { get; }

        string Include { get; }

        string ExcludeByAttribute { get; }

        string ExcludeByFile { get; }

        string IncludeDirectory { get; }

        string SingleHit { get; }

        string UseSourceLink { get; }

        string IncludeTestAssembly { get; }

        string SkipAutoProps { get; }

        bool Read(string runSettingsXml);
    }
}
