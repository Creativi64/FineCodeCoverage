using System.Linq;

namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes
{
    internal sealed class CoverageContentTypes : ICoverageContentTypes
    {
        private readonly ICoverageContentType[] _coverageContentTypes;

        public CoverageContentTypes(ICoverageContentType[] coverageContentTypes)
            => _coverageContentTypes = coverageContentTypes;

        public bool IsApplicable(string contentTypeName)
            => _coverageContentTypes.Any(contentType => contentType.ContentTypeName == contentTypeName);
    }
}
