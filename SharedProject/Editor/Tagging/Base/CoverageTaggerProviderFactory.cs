using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.IndicatorVisibility;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.EditorCoverageColouring;
using FineCodeCoverage.Utilities.Events;
using Microsoft.VisualStudio.Text.Tagging;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ICoverageTaggerProviderFactory))]
    internal sealed class CoverageTaggerProviderFactory : ICoverageTaggerProviderFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> _editorCoverageColouringOptionsProvider;
        private readonly IDynamicLineAndSnapshotSpansLogic _dynamicLineAndSnapshotSpansLogic;
        private readonly IDynamicCoverageManager _dynamicCoverageManager;
        private readonly ITextInfoFactory _textInfoFactory;
        private readonly IFileExcluder[] _fileExcluders;
        private readonly IFileIndicatorVisibility _fileIndicatorVisibility;
        private readonly IDynamicLineFilter _dynamicLineFilter;

        [ImportingConstructor]
        public CoverageTaggerProviderFactory(
            IEventAggregator eventAggregator,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic,
            IDynamicCoverageManager dynamicCoverageManager,
            ITextInfoFactory textInfoFactory,
            [ImportMany]
            IFileExcluder[] fileExcluders,
            IFileIndicatorVisibility fileIndicatorVisibility,
            IDynamicLineFilter dynamicLineFilter)
        {
            _eventAggregator = eventAggregator;
            _editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            _dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            _dynamicCoverageManager = dynamicCoverageManager;
            _textInfoFactory = textInfoFactory;
            _fileExcluders = fileExcluders;
            _fileIndicatorVisibility = fileIndicatorVisibility;
            _dynamicLineFilter = dynamicLineFilter;
        }

        public ICoverageTaggerProvider<TTag> Create<TTag, TCoverageTypeFilter>(ILineSpanTagger<TTag> tagger)
            where TTag : ITag
            where TCoverageTypeFilter : ICoverageTypeFilter, new()
                => new CoverageTaggerProvider<TCoverageTypeFilter, TTag>(
                    _eventAggregator,
                    _editorCoverageColouringOptionsProvider,
                    _dynamicLineAndSnapshotSpansLogic,
                    tagger,
                    _dynamicCoverageManager,
                    _textInfoFactory,
                    _fileExcluders,
                    _fileIndicatorVisibility,
                    _dynamicLineFilter);
    }
}
