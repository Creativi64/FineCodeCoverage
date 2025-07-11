using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Editor.Roslyn;
using FineCodeCoverage.VSAbstractions.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Blazor
{
    [Export(typeof(IBlazorFileCodeSpanRangeService))]
    internal sealed class BlazorFileCodeSpanRangeService : IBlazorFileCodeSpanRangeService
    {
        private readonly IBlazorGeneratedDocumentRootFinder _blazorGeneratedDocumentRootFinder;
        private readonly ICSharpCodeCoverageNodeVisitor _cSharpCodeCoverageNodeVisitor;
        private readonly ISyntaxNodeLocationMapper _syntaxNodeLocationMapper;
        private readonly ITextInfoFactory _textInfoFactory;
        private readonly IBlazorGeneratedFilePathMatcher _blazorGeneratedFilePathMatcher;
        private readonly IThreadHelper _threadHelper;

        [ImportingConstructor]
        public BlazorFileCodeSpanRangeService(
            IBlazorGeneratedDocumentRootFinder blazorGeneratedDocumentRootFinder,
            ICSharpCodeCoverageNodeVisitor cSharpCodeCoverageNodeVisitor,
            ISyntaxNodeLocationMapper syntaxNodeLocationMapper,
            ITextInfoFactory textInfoFactory,
            IBlazorGeneratedFilePathMatcher blazorGeneratedFilePathMatcher,
            IThreadHelper threadHelper)
        {
            _blazorGeneratedDocumentRootFinder = blazorGeneratedDocumentRootFinder;
            _cSharpCodeCoverageNodeVisitor = cSharpCodeCoverageNodeVisitor;
            _syntaxNodeLocationMapper = syntaxNodeLocationMapper;
            _textInfoFactory = textInfoFactory;
            _blazorGeneratedFilePathMatcher = blazorGeneratedFilePathMatcher;
            _threadHelper = threadHelper;
        }

        public List<CodeSpanRange> GetFileCodeSpanRanges(ITextSnapshot snapshot)
        {
            string filePath = _textInfoFactory.GetFilePath(snapshot.TextBuffer);
            SyntaxNode generatedDocumentSyntaxRoot = _threadHelper.JoinableTaskFactory.Run(
                () => _blazorGeneratedDocumentRootFinder.FindSyntaxRootAsync(snapshot.TextBuffer, filePath, _blazorGeneratedFilePathMatcher));
            if (generatedDocumentSyntaxRoot == null)
            {
                return null;
            }

            List<SyntaxNode> nodes = _cSharpCodeCoverageNodeVisitor.GetNodes(generatedDocumentSyntaxRoot);
            if (nodes.Count == 0)
            {
                return null; // sometimes the generated document has not been generated
            }

            return nodes.Select(node => new { Node = node, MappedLineSpan = _syntaxNodeLocationMapper.Map(node) })
                .Where(a => a.MappedLineSpan.Path == filePath)
                .Select(a => new CodeSpanRange(
                    a.MappedLineSpan.StartLinePosition.Line,
                    a.MappedLineSpan.EndLinePosition.Line))
                .ToList();
        }
    }
}
