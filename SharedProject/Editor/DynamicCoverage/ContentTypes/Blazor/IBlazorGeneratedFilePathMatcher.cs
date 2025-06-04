namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Blazor
{
    internal interface IBlazorGeneratedFilePathMatcher
    {
        bool IsBlazorGeneratedFilePath(string razorFilePath, string generatedfilePath);
    }
}
