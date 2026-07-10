using System.ComponentModel.Composition;
using System.Windows.Media;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Blazor;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes.Roslyn;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Editor.Tagging.Base;
using FineCodeCoverage.Utilities.Events;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Editor.Tagging.GlyphMargin
{
    [ContentType(CSharpCoverageContentType.ContentType)]
    [ContentType(VBCoverageContentType.ContentType)]
    [ContentType(CPPCoverageContentType.ContentType)]
    [ContentType(BlazorCoverageContentType.ContentType)]
    [TagType(typeof(CoverageLineGlyphTag))]
    [Name("FineCodeCoverage.TaggerProvider")]
    [Export(typeof(IViewTaggerProvider))]
    internal sealed class CoverageLineGlyphTaggerProvider : IViewTaggerProvider, ILineSpanTagger<CoverageLineGlyphTag>
    {
        private readonly ICoverageTaggerProvider<CoverageLineGlyphTag> _coverageTaggerProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoverageColoursProvider _coverageColoursProvider;

        [ImportingConstructor]
        public CoverageLineGlyphTaggerProvider(
            IEventAggregator eventAggregator,
            ICoverageColoursProvider coverageColoursProvider,
            ICoverageTaggerProviderFactory coverageTaggerProviderFactory)
        {
            _coverageTaggerProvider = coverageTaggerProviderFactory.Create<CoverageLineGlyphTag, GlyphFilter>(this);
            _eventAggregator = eventAggregator;
            _coverageColoursProvider = coverageColoursProvider;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer)
            where T : ITag
        {
            ICoverageTagger<CoverageLineGlyphTag> coverageTagger = _coverageTaggerProvider.CreateTagger(textView, buffer);
            return coverageTagger == null ? null : new CoverageLineGlyphTagger(_eventAggregator, coverageTagger) as ITagger<T>;
        }

        public TagSpan<CoverageLineGlyphTag> GetTagSpan(IDynamicLineAndSnapshotSpan dynamicLineAndSnapshotSpan)
        {
            ICoverageColours coverageColours = _coverageColoursProvider.GetCoverageColours();
            Color colour = coverageColours.GetColour(dynamicLineAndSnapshotSpan.Line.CoverageType).Background;
            return new TagSpan<CoverageLineGlyphTag>(dynamicLineAndSnapshotSpan.Span, new CoverageLineGlyphTag(colour));
        }
    }
}
