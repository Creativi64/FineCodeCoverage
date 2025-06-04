using System.ComponentModel.Composition;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;

namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Roslyn
{
    [Export(typeof(ICoverageContentType))]
    internal class CSharpCoverageContentType : ICoverageContentType
    {
        private readonly IRoslynFileCodeSpanRangeService _roslynFileCodeSpanRangeService;

        [ImportingConstructor]
        public CSharpCoverageContentType(IRoslynFileCodeSpanRangeService roslynFileCodeSpanRangeService)
            => _roslynFileCodeSpanRangeService = roslynFileCodeSpanRangeService;

        public const string ContentType = "CSharp";

        public string ContentTypeName => ContentType;

        public IFileCodeSpanRangeService FileCodeSpanRangeService
            => _roslynFileCodeSpanRangeService.FileCodeSpanRangeService;

        public bool UseFileCodeSpanRangeServiceForChanges
            => _roslynFileCodeSpanRangeService.UseFileCodeSpanRangeServiceForChanges;

        public bool CoverageOnlyFromFileCodeSpanRangeService => false;

        public static string[] Exclusions { get; } = new string[] { "//", "#", "using" };

        public ILineExcluder LineExcluder { get; } = new LineExcluder(Exclusions);

    }
}
