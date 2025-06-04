using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class CoverageContentTypes : ICoverageContentTypes
    {
        private readonly ICoverageContentType[] _coverageContentTypes;

        public CoverageContentTypes(ICoverageContentType[] coverageContentTypes)
            => this._coverageContentTypes = coverageContentTypes;

        public bool IsApplicable(string contentTypeName)
            => this._coverageContentTypes.Any(contentType => contentType.ContentTypeName == contentTypeName);
    }
}
