using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class CoverageContentTypes : ICoverageContentTypes
    {
        private readonly ICoverageContentType[] coverageContentTypes;

        public CoverageContentTypes(ICoverageContentType[] coverageContentTypes)
            => this.coverageContentTypes = coverageContentTypes;
        public bool IsApplicable(string contentTypeName)
            => this.coverageContentTypes.Any(contentType => contentType.ContentTypeName == contentTypeName);
    }
}
