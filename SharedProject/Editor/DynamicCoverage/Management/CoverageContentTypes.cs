using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;

namespace FineCodeCoverage.Editor.DynamicCoverage
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
