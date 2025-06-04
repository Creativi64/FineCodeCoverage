namespace FineCodeCoverage.Core.Utilities
{
    internal interface IVsVersion
    {
        bool Is2022 { get; }
        string GetSemanticVersion();
        string GetReleaseVersion();
        string GetDisplayVersion();
        string GetEditionName();
    }
}
