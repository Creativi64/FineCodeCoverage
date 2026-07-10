namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    internal interface IDynamicCoverageManager
    {
        IBufferLineCoverage Manage(ITextInfo textInfo);
    }
}
