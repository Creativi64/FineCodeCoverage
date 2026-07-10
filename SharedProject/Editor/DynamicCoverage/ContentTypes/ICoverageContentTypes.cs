namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes
{
    internal interface ICoverageContentTypes
    {
        bool IsApplicable(string contentTypeName);
    }
}
