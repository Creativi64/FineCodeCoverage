namespace FineCodeCoverage.Core.Utilities
{
    public interface IVsVersion
    {
        bool Is2022 { get; }

        string GetSemanticVersion();

        string GetReleaseVersion();

        string GetDisplayVersion();

        string GetEditionName();
    }
}
