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
        private readonly ITrackingSpanFactory _trackingLineFactory;
        private readonly ITrackingSpanRangeFactory _trackingSpanRangeFactory;
        private readonly ITrackedCoverageLinesFactory _trackedCoverageLinesFactory;
        private readonly ITrackedCoverageLineFactory _trackedCoverageLineFactory;
        private readonly ITrackingSpanRangeContainingCodeTrackerFactory _trackingSpanRangeContainingCodeTrackerFactory;
        private readonly INotIncludedLineFactory _notIncludedLineFactory;

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
            this._trackingLineFactory = trackingLineFactory;
            this._trackingSpanRangeFactory = trackingSpanRangeFactory;
            this._trackedCoverageLinesFactory = trackedCoverageLinesFactory;
            this._trackedCoverageLineFactory = trackedCoverageLineFactory;
            this._trackingSpanRangeContainingCodeTrackerFactory = trackingSpanRangeContainingCodeTrackerFactory;
            this._notIncludedLineFactory = notIncludedLineFactory;
        }

        public IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            ITrackingLine notIncludedLine = this._notIncludedLineFactory.Create(trackingSpanRange.GetFirstTrackingSpan(), textSnapshot);
            return this._trackingSpanRangeContainingCodeTrackerFactory.CreateNotIncluded(notIncludedLine, trackingSpanRange);
        }

        public IContainingCodeTracker CreateCoverageLines(
            ITextSnapshot textSnapshot,
            List<ICoberturaLine> coberturaLines,
            CodeSpanRange containingRange,
            SpanTrackingMode spanTrackingMode
        ) => this._trackingSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
            this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode),
            this.CreateTrackedCoverageLines(textSnapshot, coberturaLines, spanTrackingMode)
        );

        public IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = this.CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            return this._trackingSpanRangeContainingCodeTrackerFactory.CreateOtherLines(trackingSpanRange);
        }

        private ITrackingSpanRange CreateTrackingSpanRange(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpan startTrackingSpan = this._trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.StartLine, spanTrackingMode);
            ITrackingSpan endTrackingSpan = this._trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.EndLine, spanTrackingMode);
            return this._trackingSpanRangeFactory.Create(startTrackingSpan, endTrackingSpan, textSnapshot);
        }

        private ITrackedCoverageLines CreateTrackedCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, SpanTrackingMode spanTrackingMode)
        {
            List<ITrackedCoverageLine> trackedCoverageLines = coberturaLines.ConvertAll(coberturaLine => this._trackedCoverageLineFactory.Create(
                this._trackingLineFactory.CreateTrackingSpan(textSnapshot, coberturaLine.Number - 1, spanTrackingMode), coberturaLine)
            );
            return this._trackedCoverageLinesFactory.Create(trackedCoverageLines.ToList());
        }
    }
}
