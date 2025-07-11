using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.EditorCoverageColouring;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IBufferLineCoverageFactory))]
    internal sealed class BufferLineCoverageFactory : IBufferLineCoverageFactory
    {
        private readonly ICoverageContentTypes _coverageContentTypes;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> _editorCoverageColouringOptionsProvider;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public BufferLineCoverageFactory(
            [ImportMany]
            ICoverageContentType[] coverageContentTypes,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            ILogger logger)
        {
            _editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            _logger = logger;
            _coverageContentTypes = new CoverageContentTypes(coverageContentTypes);
        }

        public IBufferLineCoverage Create(
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory) => new BufferLineCoverage(
                textInfo,
                eventAggregator,
                trackedLinesFactory,
                _editorCoverageColouringOptionsProvider,
                _coverageContentTypes,
                _logger);
    }
}
