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
        private readonly ICoverageContentTypes coverageContentTypes;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider;
        private readonly ILogger logger;

        [ImportingConstructor]
        public BufferLineCoverageFactory(
            [ImportMany]
            ICoverageContentType[] coverageContentTypes,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            ILogger logger
        )
        {
            this.editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            this.logger = logger;
            this.coverageContentTypes = new CoverageContentTypes(coverageContentTypes);
        }

        public IBufferLineCoverage Create(
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory
        ) =>  new BufferLineCoverage(
                textInfo,
                eventAggregator,
                trackedLinesFactory,
                this.editorCoverageColouringOptionsProvider,
                this.coverageContentTypes,
                this.logger
                );
    }
}
