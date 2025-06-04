using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedLines : IContainingCodeTrackerTrackedLines
    {
        private readonly List<IContainingCodeTracker> _containingCodeTrackers;
        private readonly IFileCodeSpanRangeService _fileCodeSpanRangeService;
        private readonly bool _useFileCodeSpanRangeService;
        public IReadOnlyList<IContainingCodeTracker> ContainingCodeTrackers => this._containingCodeTrackers;

        public INewCodeTracker NewCodeTracker { get; }

        public TrackedLines(
            List<IContainingCodeTracker> containingCodeTrackers,
            INewCodeTracker newCodeTracker,
            IFileCodeSpanRangeService fileCodeSpanRangeService)
        {
            this._containingCodeTrackers = containingCodeTrackers;
            this.NewCodeTracker = newCodeTracker;
            this._fileCodeSpanRangeService = fileCodeSpanRangeService;
            this._useFileCodeSpanRangeService = this._fileCodeSpanRangeService != null && newCodeTracker != null;
        }

        private static List<LineRange> GetRanges(ITextSnapshot currentSnapshot, List<Span> newSpanChanges)
            => newSpanChanges.ConvertAll(
                 newSpanChange => new LineRange(
                     currentSnapshot.GetLineNumberFromPosition(newSpanChange.Start),
                     currentSnapshot.GetLineNumberFromPosition(newSpanChange.End)
                 ));

        private (IEnumerable<int>, List<LineRange>) ProcessContainingCodeTrackers(
            ITextSnapshot currentSnapshot,
            List<LineRange> spanAndLineRanges
            )
        {
            var removals = new List<IContainingCodeTracker>();
            var allChangedLines = new List<int>();
            foreach (IContainingCodeTracker containingCodeTracker in this._containingCodeTrackers)
            {
                (IEnumerable<int> changedLines, List<LineRange> unprocessedSpans) =
                    ProcessContainingCodeTracker(removals, containingCodeTracker, currentSnapshot, spanAndLineRanges);
                allChangedLines.AddRange(changedLines);
                spanAndLineRanges = unprocessedSpans;
            }

            removals.ForEach(removal =>
            {
                removal.Deleted();
                _ = this._containingCodeTrackers.Remove(removal);
            });

            return (allChangedLines, spanAndLineRanges);
        }

        private static (IEnumerable<int> changedLines, List<LineRange> unprocessedSpans) ProcessContainingCodeTracker(
            List<IContainingCodeTracker> removals,
            IContainingCodeTracker containingCodeTracker,
            ITextSnapshot currentSnapshot,
            List<LineRange> spanAndLineRanges
        )
        {
            IContainingCodeTrackerProcessResult processResult = containingCodeTracker.ProcessChanges(currentSnapshot, spanAndLineRanges);
            if (processResult.IsEmpty)
            {
                removals.Add(containingCodeTracker);
            }

            return (processResult.ChangedLines, processResult.UnprocessedSpans);
        }

        // normalized spans
        public IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<Span> newSpanChanges)
        {
            List<LineRange> changedRanges = GetRanges(currentSnapshot, newSpanChanges);
            (IEnumerable<int> changedLines, List<LineRange> unprocessedRanges) =
                this.ProcessContainingCodeTrackers(currentSnapshot, changedRanges);
            IEnumerable<int> newCodeTrackerChangedLines = this.GetNewCodeTrackerChangedLineNumbers(
                currentSnapshot, unprocessedRanges);
            return changedLines.Concat(newCodeTrackerChangedLines).Distinct();
        }

        private IEnumerable<int> GetNewCodeTrackerChangedLineNumbers(ITextSnapshot currentSnapshot, List<LineRange> ranges)
            => this.NewCodeTracker == null ?
            Enumerable.Empty<int>() : this.GetNewCodeTrackerChangedLineNumbersActual(currentSnapshot, ranges);

        private IEnumerable<int> GetNewCodeTrackerChangedLineNumbersActual(ITextSnapshot currentSnapshot, List<LineRange> ranges)
            => this._useFileCodeSpanRangeService
                ? this.NewCodeTracker.GetChangedLineNumbers(currentSnapshot, this.GetNewCodeCodeRanges(currentSnapshot))
                : this.NewCodeTracker.GetChangedLineNumbers(currentSnapshot, ranges);

        private List<CodeSpanRange> GetNewCodeCodeRanges(ITextSnapshot currentSnapshot)
            => this.GetNewCodeCodeRanges(currentSnapshot, this.GetContainingCodeTrackersCodeSpanRanges()).ToList();

        private List<CodeSpanRange> GetContainingCodeTrackersCodeSpanRanges()
            => this._containingCodeTrackers.ConvertAll(ct => ct.GetState().CodeSpanRange);

        private List<CodeSpanRange> GetNewCodeCodeRanges(
            ITextSnapshot currentSnapshot,
            List<CodeSpanRange> containingCodeTrackersCodeSpanRanges)
        {
            List<CodeSpanRange> fileCodeSpanRanges = this._fileCodeSpanRangeService.GetFileCodeSpanRanges(currentSnapshot);
            var newCodeCodeRanges = new List<CodeSpanRange>();
            int i = 0, j = 0;

            containingCodeTrackersCodeSpanRanges = containingCodeTrackersCodeSpanRanges.OrderBy(codeSpanRange => codeSpanRange.StartLine).ToList();
            while (i < fileCodeSpanRanges.Count && j < containingCodeTrackersCodeSpanRanges.Count)
            {
                CodeSpanRange fileRange = fileCodeSpanRanges[i];
                CodeSpanRange trackerRange = containingCodeTrackersCodeSpanRanges[j];

                if (fileRange.EndLine < trackerRange.StartLine)
                {
                    // fileRange does not intersect with trackerRange, add it to the result
                    newCodeCodeRanges.Add(fileRange);
                    i++;
                }
                else if (fileRange.StartLine > trackerRange.EndLine)
                {
                    // fileRange is after trackerRange, move to the next trackerRange
                    j++;
                }
                else
                {
                    // fileRange intersects with trackerRange, skip it
                    i++;
                }
            }

            // Add remaining fileCodeSpanRanges that come after the last trackerRange
            while (i < fileCodeSpanRanges.Count)
            {
                newCodeCodeRanges.Add(fileCodeSpanRanges[i]);
                i++;
            }

            return newCodeCodeRanges;
        }

        private static (bool done, IEnumerable<IDynamicLine> lines) GetLines(IEnumerable<IDynamicLine> dynamicLines, int startLineNumber, int endLineNumber)
        {
            IEnumerable<IDynamicLine> linesApplicableToStartLineNumber = LinesApplicableToStartLineNumber(dynamicLines, startLineNumber);
            var lines = linesApplicableToStartLineNumber.TakeWhile(l => l.LineNumber <= endLineNumber).ToList();
            bool done = lines.Count != linesApplicableToStartLineNumber.Count();
            return (done, lines);
        }

        private static IEnumerable<IDynamicLine> LinesApplicableToStartLineNumber(
            IEnumerable<IDynamicLine> dynamicLines, int startLineNumber
        ) => dynamicLines.Where(l => l.LineNumber >= startLineNumber);

        private IEnumerable<IDynamicLine> GetLinesFromContainingCodeTrackers(int startLineNumber, int endLineNumber)
            => this._containingCodeTrackers.Select(containingCodeTracker => GetLines(containingCodeTracker.Lines, startLineNumber, endLineNumber))
                .TakeUntil(a => a.done).SelectMany(a => a.lines);

        private IEnumerable<IDynamicLine> NewCodeTrackerLines() => this.NewCodeTracker?.Lines ?? Enumerable.Empty<IDynamicLine>();

        private IEnumerable<IDynamicLine> GetNewLines(int startLineNumber, int endLineNumber)
            => LinesApplicableToStartLineNumber(this.NewCodeTrackerLines(), startLineNumber)
                .TakeWhile(l => l.LineNumber <= endLineNumber);

        public IEnumerable<IDynamicLine> GetLines(int startLineNumber, int endLineNumber)
            => this.GetLinesFromContainingCodeTrackers(startLineNumber, endLineNumber)
                .Concat(this.GetNewLines(startLineNumber, endLineNumber))
                .Distinct(new DynamicLineByLineNumberComparer()).ToList();

        private class DynamicLineByLineNumberComparer : IEqualityComparer<IDynamicLine>
        {
            public bool Equals(IDynamicLine x, IDynamicLine y) => x.LineNumber == y.LineNumber;
            public int GetHashCode(IDynamicLine obj) => obj.LineNumber;
        }
    }
}