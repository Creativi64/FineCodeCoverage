namespace FineCodeCoverage.VSAbstractions.Versioning
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
