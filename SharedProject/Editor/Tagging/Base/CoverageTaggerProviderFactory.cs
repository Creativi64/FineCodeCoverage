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
        private readonly IEventAggregator eventAggregator;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider;
        private readonly IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic;
        private readonly IDynamicCoverageManager dynamicCoverageManager;
        private readonly ITextInfoFactory textInfoFactory;
        private readonly IFileExcluder[] fileExcluders;
        private readonly IFileIndicatorVisibility fileIndicatorVisibility;
        private readonly IDynamicLineFilter dynamicLineFilter;

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
            this.eventAggregator = eventAggregator;
            this.editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            this.dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            this.dynamicCoverageManager = dynamicCoverageManager;
            this.textInfoFactory = textInfoFactory;
            this.fileExcluders = fileExcluders;
            this.fileIndicatorVisibility = fileIndicatorVisibility;
            this.dynamicLineFilter = dynamicLineFilter;
        }
        public ICoverageTaggerProvider<TTag> Create<TTag, TCoverageTypeFilter>(ILineSpanTagger<TTag> tagger)
            where TTag : ITag
            where TCoverageTypeFilter : ICoverageTypeFilter, new()
                => new CoverageTaggerProvider<TCoverageTypeFilter, TTag>(
                    this.eventAggregator,
                    this.editorCoverageColouringOptionsProvider,
                    this.dynamicLineAndSnapshotSpansLogic,
                    tagger,
                    this.dynamicCoverageManager,
                    this.textInfoFactory,
                    this.fileExcluders,
                    this.fileIndicatorVisibility,
                    this.dynamicLineFilter
                );
    }
}
