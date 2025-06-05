using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Editor.Roslyn;
using FineCodeCoverage.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Roslyn
{
    [Export(typeof(IRoslynFileCodeSpanRangeService))]
    internal class RoslynFileCodeSpanRangeService : IFileCodeSpanRangeService, IRoslynFileCodeSpanRangeService
    {
        private readonly IRoslynService _roslynService;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> _editorCoverageColouringOptionsProvider;
        private readonly IThreadHelper _threadHelper;

        [ImportingConstructor]
        public RoslynFileCodeSpanRangeService(
            IRoslynService roslynService,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            IThreadHelper threadHelper)
        {
            _roslynService = roslynService;
            _editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            _threadHelper = threadHelper;
        }

        private static CodeSpanRange GetCodeSpanRange(TextSpan span, ITextSnapshot textSnapshot)
        {
            int startLine = textSnapshot.GetLineNumberFromPosition(span.Start);
            int endLine = textSnapshot.GetLineNumberFromPosition(span.End);
            return new CodeSpanRange(startLine, endLine);
        }

        public List<CodeSpanRange> GetFileCodeSpanRanges(ITextSnapshot snapshot)
        {
            List<TextSpan> textSpans = _threadHelper.JoinableTaskFactory.Run(
                () => _roslynService.GetContainingCodeSpansAsync(snapshot));

            return textSpans.ConvertAll(textSpan => GetCodeSpanRange(textSpan, snapshot));
        }

        public IFileCodeSpanRangeService FileCodeSpanRangeService => this;

        public bool UseFileCodeSpanRangeServiceForChanges
            => _editorCoverageColouringOptionsProvider.Get().EditorCoverageColouringMode != EditorCoverageColouringMode.DoNotUseRoslynWhenTextChanges;
    }
}
