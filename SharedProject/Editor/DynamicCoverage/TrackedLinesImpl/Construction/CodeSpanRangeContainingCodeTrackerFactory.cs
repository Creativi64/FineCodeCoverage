using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(ICodeSpanRangeContainingCodeTrackerFactory))]
    internal class CodeSpanRangeContainingCodeTrackerFactory : ICodeSpanRangeContainingCodeTrackerFactory
    {
        private readonly ITrackingSpanFactory trackingLineFactory;
        private readonly ITrackingSpanRangeFactory trackingSpanRangeFactory;
        private readonly ITrackedCoverageLinesFactory trackedCoverageLinesFactory;
        private readonly ITrackedCoverageLineFactory trackedCoverageLineFactory;
        private readonly ITrackingSpanRangeContainingCodeTrackerFactory trackingSpanRangeContainingCodeTrackerFactory;
        private readonly INotIncludedLineFactory notIncludedLineFactory;

        [ImportingConstructor]
        public CodeSpanRangeContainingCodeTrackerFactory(
            ITrackingSpanFactory trackingLineFactory,
            ITrackingSpanRangeFactory trackingSpanRangeFactory,
            ITrackedCoverageLinesFactory trackedCoverageLinesFactory,
            ITrackedCoverageLineFactory trackedCoverageLineFactory,
            ITrackingSpanRangeContainingCodeTrackerFactory trackingSpanRangeContainingCodeTrackerFactory,
            INotIncludedLineFactory notIncludedLineFactory
            )
        {
            this.trackingLineFactory = trackingLineFactory;
            this.trackingSpanRangeFactory = trackingSpanRangeFactory;
            this.trackedCoverageLinesFactory = trackedCoverageLinesFactory;
            this.trackedCoverageLineFactory = trackedCoverageLineFactory;
            this.trackingSpanRangeContainingCodeTrackerFactory = trackingSpanRangeContainingCodeTrackerFactory;
            this.notIncludedLineFactory = notIncludedLineFactory;
        }

        public IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            ITrackingLine notIncludedLine = this.notIncludedLineFactory.Create(trackingSpanRange.GetFirstTrackingSpan(), textSnapshot);
            return this.trackingSpanRangeContainingCodeTrackerFactory.CreateNotIncluded(notIncludedLine, trackingSpanRange);
        }

        public IContainingCodeTracker CreateCoverageLines(
            ITextSnapshot textSnapshot,
            List<ICoberturaLine> coberturaLines,
            CodeSpanRange containingRange,
            SpanTrackingMode spanTrackingMode
        ) => this.trackingSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
            this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode),
            this.CreateTrackedCoverageLines(textSnapshot, coberturaLines, spanTrackingMode)
        );

        public IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            return this.trackingSpanRangeContainingCodeTrackerFactory.CreateOtherLines(trackingSpanRange);
        }

        private ITrackingSpanRange CreateTrackingSpanRange(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpan startTrackingSpan = this.trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.StartLine, spanTrackingMode);
            ITrackingSpan endTrackingSpan = this.trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.EndLine, spanTrackingMode);
            return this.trackingSpanRangeFactory.Create(startTrackingSpan, endTrackingSpan, textSnapshot);
        }

        private ITrackedCoverageLines CreateTrackedCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, SpanTrackingMode spanTrackingMode)
        {
            List<ITrackedCoverageLine> trackedCoverageLines = coberturaLines.ConvertAll(coberturaLine => this.trackedCoverageLineFactory.Create(
                this.trackingLineFactory.CreateTrackingSpan(textSnapshot, coberturaLine.Number - 1, spanTrackingMode), coberturaLine)
            );
            return this.trackedCoverageLinesFactory.Create(trackedCoverageLines.ToList());
        }
    }
}
