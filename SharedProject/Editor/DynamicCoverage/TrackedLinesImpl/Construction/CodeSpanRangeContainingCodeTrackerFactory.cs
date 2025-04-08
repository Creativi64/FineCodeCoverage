using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(ICodeSpanRangeContainingCodeTrackerFactory))]
    internal class CodeSpanRangeContainingCodeTrackerFactory : ICodeSpanRangeContainingCodeTrackerFactory
    {
        private readonly ITrackingLineFactory trackingLineFactory;
        private readonly ITrackingSpanRangeFactory trackingSpanRangeFactory;
        private readonly ITrackedCoverageLinesFactory trackedCoverageLinesFactory;
        private readonly ICoverageLineFactory coverageLineFactory;
        private readonly ITrackingSpanRangeContainingCodeTrackerFactory trackedContainingCodeTrackerFactory;
        private readonly INotIncludedLineFactory notIncludedLineFactory;

        [ImportingConstructor]
        public CodeSpanRangeContainingCodeTrackerFactory(
            ITrackingLineFactory trackingLineFactory,
            ITrackingSpanRangeFactory trackingSpanRangeFactory,
            ITrackedCoverageLinesFactory trackedCoverageLinesFactory,
            ICoverageLineFactory coverageLineFactory,
            ITrackingSpanRangeContainingCodeTrackerFactory trackedContainingCodeTrackerFactory,
            INotIncludedLineFactory notIncludedLineFactory
            )
        {
            this.trackingLineFactory = trackingLineFactory;
            this.trackingSpanRangeFactory = trackingSpanRangeFactory;
            this.trackedCoverageLinesFactory = trackedCoverageLinesFactory;
            this.coverageLineFactory = coverageLineFactory;
            this.trackedContainingCodeTrackerFactory = trackedContainingCodeTrackerFactory;
            this.notIncludedLineFactory = notIncludedLineFactory;
        }

        public IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            ITrackingLine notIncludedLine = this.notIncludedLineFactory.Create(trackingSpanRange.GetFirstTrackingSpan(), textSnapshot);
            return this.trackedContainingCodeTrackerFactory.CreateNotIncluded(notIncludedLine, trackingSpanRange);
        }

        public IContainingCodeTracker CreateCoverageLines(
            ITextSnapshot textSnapshot,
            List<ICoberturaLine> coberturaLines,
            CodeSpanRange containingRange,
            SpanTrackingMode spanTrackingMode
        ) => this.trackedContainingCodeTrackerFactory.CreateCoverageLines(
            this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode),
            this.CreateTrackedCoverageLines(textSnapshot, coberturaLines, spanTrackingMode)
        );

        public IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            return this.trackedContainingCodeTrackerFactory.CreateOtherLines(trackingSpanRange);
        }

        private ITrackingSpanRange CreateTrackingSpanRange(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpan startTrackingSpan = this.trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.StartLine, spanTrackingMode);
            ITrackingSpan endTrackingSpan = this.trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.EndLine, spanTrackingMode);
            return this.trackingSpanRangeFactory.Create(startTrackingSpan, endTrackingSpan, textSnapshot);
        }

        private ITrackedCoverageLines CreateTrackedCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, SpanTrackingMode spanTrackingMode)
        {
            List<ICoverageLine> coverageLines = coberturaLines.ConvertAll(coberturaLine => this.coverageLineFactory.Create(
                this.trackingLineFactory.CreateTrackingSpan(textSnapshot, coberturaLine.Number - 1, spanTrackingMode), coberturaLine)
            );
            return this.trackedCoverageLinesFactory.Create(coverageLines.ToList());
        }

        public IContainingCodeTracker CreateDirty(
            ITextSnapshot currentSnapshot,
            CodeSpanRange containingRange,
            SpanTrackingMode spanTrackingMode
        ) => this.trackedContainingCodeTrackerFactory.CreateDirty(
            this.CreateTrackingSpanRange(currentSnapshot, containingRange, spanTrackingMode),
            currentSnapshot);
    }
}
