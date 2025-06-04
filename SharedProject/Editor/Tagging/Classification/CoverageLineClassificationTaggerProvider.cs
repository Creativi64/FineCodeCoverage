using System.ComponentModel.Composition;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Blazor;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Roslyn;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Editor.Tagging.Base;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Editor.Tagging.Classification
{
    [ContentType(CSharpCoverageContentType.ContentType)]
    [ContentType(VBCoverageContentType.ContentType)]
    [ContentType(CPPCoverageContentType.ContentType)]
    [ContentType(BlazorCoverageContentType.ContentType)]
    [TagType(typeof(IClassificationTag))]
    [Name("FCC.CoverageLineClassificationTaggerProvider")]
    [Export(typeof(IViewTaggerProvider))]
    internal class CoverageLineClassificationTaggerProvider : IViewTaggerProvider, ILineSpanTagger<IClassificationTag>
    {
        private readonly ICoverageTypeService _coverageTypeService;
        private readonly ICoverageTaggerProvider<IClassificationTag> _coverageTaggerProvider;

        [ImportingConstructor]
        public CoverageLineClassificationTaggerProvider(
            ICoverageTypeService coverageTypeService,
            ICoverageTaggerProviderFactory coverageTaggerProviderFactory
        )
        {
            this._coverageTypeService = coverageTypeService;
            this._coverageTaggerProvider = coverageTaggerProviderFactory.Create<IClassificationTag, CoverageClassificationFilter>(this);
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
            => this._coverageTaggerProvider.CreateTagger(textView, buffer) as ITagger<T>;

        public TagSpan<IClassificationTag> GetTagSpan(IDynamicLineAndSnapshotSpan dynamicLineAndSnapshotSpan)
        {
            IClassificationType ct = this._coverageTypeService.GetClassificationType(dynamicLineAndSnapshotSpan.Line.CoverageType);
            return new TagSpan<IClassificationTag>(dynamicLineAndSnapshotSpan.Span, new ClassificationTag(ct));
        }
    }
}
