using System.ComponentModel.Composition;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;

namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Roslyn
{
    [Export(typeof(ICoverageContentType))]
    internal class VBCoverageContentType : ICoverageContentType
    {
        private readonly IRoslynFileCodeSpanRangeService _roslynFileCodeSpanRangeService;

        [ImportingConstructor]
        public VBCoverageContentType(IRoslynFileCodeSpanRangeService roslynFileCodeSpanRangeService)
            => this._roslynFileCodeSpanRangeService = roslynFileCodeSpanRangeService;

        public const string ContentType = "Basic";

        public string ContentTypeName => ContentType;

        public IFileCodeSpanRangeService FileCodeSpanRangeService
            => this._roslynFileCodeSpanRangeService.FileCodeSpanRangeService;

        public bool CoverageOnlyFromFileCodeSpanRangeService => false;

        public bool UseFileCodeSpanRangeServiceForChanges
            => this._roslynFileCodeSpanRangeService.UseFileCodeSpanRangeServiceForChanges;

        public ILineExcluder LineExcluder { get; } = new LineExcluder(new string[] { "REM", "'", "#" });
    }
}
