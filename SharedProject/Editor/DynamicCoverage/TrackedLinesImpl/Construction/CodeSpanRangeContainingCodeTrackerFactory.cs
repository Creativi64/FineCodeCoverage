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
            INotIncludedLineFactory notIncludedLineFactory)
        {
            _trackingLineFactory = trackingLineFactory;
            _trackingSpanRangeFactory = trackingSpanRangeFactory;
            _trackedCoverageLinesFactory = trackedCoverageLinesFactory;
            _trackedCoverageLineFactory = trackedCoverageLineFactory;
            _trackingSpanRangeContainingCodeTrackerFactory = trackingSpanRangeContainingCodeTrackerFactory;
            _notIncludedLineFactory = notIncludedLineFactory;
        }

        public IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            ITrackingLine notIncludedLine = _notIncludedLineFactory.Create(trackingSpanRange.GetFirstTrackingSpan(), textSnapshot);
            return _trackingSpanRangeContainingCodeTrackerFactory.CreateNotIncluded(notIncludedLine, trackingSpanRange);
        }

        public IContainingCodeTracker CreateCoverageLines(
            ITextSnapshot textSnapshot,
            List<ICoberturaLine> coberturaLines,
            CodeSpanRange containingRange,
            SpanTrackingMode spanTrackingMode) => _trackingSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
            CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode),
            CreateTrackedCoverageLines(textSnapshot, coberturaLines, spanTrackingMode));

        public IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpanRange trackingSpanRange = CreateTrackingSpanRange(textSnapshot, containingRange, spanTrackingMode);
            return _trackingSpanRangeContainingCodeTrackerFactory.CreateOtherLines(trackingSpanRange);
        }

        private ITrackingSpanRange CreateTrackingSpanRange(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode)
        {
            ITrackingSpan startTrackingSpan = _trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.StartLine, spanTrackingMode);
            ITrackingSpan endTrackingSpan = _trackingLineFactory.CreateTrackingSpan(textSnapshot, containingRange.EndLine, spanTrackingMode);
            return _trackingSpanRangeFactory.Create(startTrackingSpan, endTrackingSpan, textSnapshot);
        }

        private ITrackedCoverageLines CreateTrackedCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, SpanTrackingMode spanTrackingMode)
        {
            List<ITrackedCoverageLine> trackedCoverageLines = coberturaLines.ConvertAll(coberturaLine => _trackedCoverageLineFactory.Create(
                _trackingLineFactory.CreateTrackingSpan(textSnapshot, coberturaLine.Number - 1, spanTrackingMode), coberturaLine));
            return _trackedCoverageLinesFactory.Create(trackedCoverageLines.ToList());
        }
    }
}
