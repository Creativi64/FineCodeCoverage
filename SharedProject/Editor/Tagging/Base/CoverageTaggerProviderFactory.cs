using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.IndicatorVisibility;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Text.Tagging;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ICoverageTaggerProviderFactory))]
    internal class CoverageTaggerProviderFactory : ICoverageTaggerProviderFactory
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
            IDynamicLineFilter dynamicLineFilter
        )
        {
            this._eventAggregator = eventAggregator;
            this._editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            this._dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            this._dynamicCoverageManager = dynamicCoverageManager;
            this._textInfoFactory = textInfoFactory;
            this._fileExcluders = fileExcluders;
            this._fileIndicatorVisibility = fileIndicatorVisibility;
            this._dynamicLineFilter = dynamicLineFilter;
        }
        public ICoverageTaggerProvider<TTag> Create<TTag, TCoverageTypeFilter>(ILineSpanTagger<TTag> tagger)
            where TTag : ITag
            where TCoverageTypeFilter : ICoverageTypeFilter, new()
                => new CoverageTaggerProvider<TCoverageTypeFilter, TTag>(
                    this._eventAggregator,
                    this._editorCoverageColouringOptionsProvider,
                    this._dynamicLineAndSnapshotSpansLogic,
                    tagger,
                    this._dynamicCoverageManager,
                    this._textInfoFactory,
                    this._fileExcluders,
                    this._fileIndicatorVisibility,
                    this._dynamicLineFilter
                );
    }
}