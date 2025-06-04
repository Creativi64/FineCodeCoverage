using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IBufferLineCoverageFactory))]
    internal class BufferLineCoverageFactory : IBufferLineCoverageFactory
    {
        private readonly ICoverageContentTypes _coverageContentTypes;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> _editorCoverageColouringOptionsProvider;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public BufferLineCoverageFactory(
            [ImportMany]
            ICoverageContentType[] coverageContentTypes,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            ILogger logger
        )
        {
            this._editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            this._logger = logger;
            this._coverageContentTypes = new CoverageContentTypes(coverageContentTypes);
        }

        public IBufferLineCoverage Create(
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory
        ) => new BufferLineCoverage(
                textInfo,
                eventAggregator,
                trackedLinesFactory,
                this._editorCoverageColouringOptionsProvider,
                this._coverageContentTypes,
                this._logger
                );
    }
}