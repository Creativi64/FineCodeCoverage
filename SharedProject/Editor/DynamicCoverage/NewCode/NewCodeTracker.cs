using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class NewCodeTracker : INewCodeTracker
    {
        private readonly List<ITrackedNewCodeLine> trackedNewCodeLines = new List<ITrackedNewCodeLine>();
        private readonly ITrackedNewCodeLineFactory trackedNewCodeLineFactory;
        private readonly ILineExcluder codeLineExcluder;
        private readonly ITextInfoFactory textInfoFactory;

        public NewCodeTracker(
            ITrackedNewCodeLineFactory trackedNewCodeLineFactory,
            ILineExcluder codeLineExcluder,
            ITextInfoFactory textInfoFactory
            )
        {
            this.trackedNewCodeLineFactory = trackedNewCodeLineFactory;
            this.codeLineExcluder = codeLineExcluder;
            this.textInfoFactory = textInfoFactory;
        }

        public event EventHandler<HasNewCodeChangedEventArgs> HasNewCodeChanged;

        public IEnumerable<IDynamicLine> Lines => this.trackedNewCodeLines.OrderBy(l => l.Line.Number).Select(l => l.Line);

        private void OnHasNewCodeChanged(bool hasNewCode, ITextSnapshot textSnapshot)
            => HasNewCodeChanged?.Invoke(
                    this,
                    new HasNewCodeChangedEventArgs(this.textInfoFactory.GetFilePath(textSnapshot.TextBuffer), hasNewCode)
            );

        private T OnHasNewCodeChangedIfChanged<T>(Func<T> action, ITextSnapshot currentSnapshot)
        {
            bool hasTracked = this.trackedNewCodeLines.Any();
            T t = action();
            bool newHasTracked = this.trackedNewCodeLines.Any();
            if (newHasTracked != hasTracked)
            {
                this.OnHasNewCodeChanged(newHasTracked, currentSnapshot);
            }

            return t;
        }

        #region CodeSpanRange

        public IEnumerable<int> GetChangedLineNumbers(ITextSnapshot textSnapshot, List<CodeSpanRange> codeSpanRanges)
    => this.OnHasNewCodeChangedIfChanged(() =>
    {
        List<int> rangeStartLineNumbers = codeSpanRanges.ConvertAll(newCodeCodeRange => newCodeCodeRange.StartLine);
        List<int> removed = this.RemoveAndReduceByDynamicLineNumbers(rangeStartLineNumbers);
        List<int> newRangeStartLineNumbers = rangeStartLineNumbers;
        this.CreateTrackedNewCodeLinesFromRangeStartLineNumbers(newRangeStartLineNumbers, textSnapshot);
        return removed.Concat(newRangeStartLineNumbers).OrderBy(lineNumber => lineNumber).ToList();
    }, textSnapshot);

        private List<int> RemoveAndReduceByDynamicLineNumbers(List<int> rangeStartLineNumbers)
        {
            var removals = this.trackedNewCodeLines.Where(
                trackedNewCodeLine => !rangeStartLineNumbers.Remove(trackedNewCodeLine.Line.Number)).ToList();

            removals.ForEach(removal => this.trackedNewCodeLines.Remove(removal));
            return removals.ConvertAll(removal => removal.Line.Number);
        }

        private void CreateTrackedNewCodeLinesFromRangeStartLineNumbers(IEnumerable<int> rangeStartLineNumbers, ITextSnapshot currentSnapshot)
            => this.trackedNewCodeLines.AddRange(
                rangeStartLineNumbers.Select(lineNumber => this.CreateTrackedNewCodeLine(currentSnapshot, lineNumber))
            );

        #endregion

        #region LineRange

        public IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<LineRange> ranges)
            => this.OnHasNewCodeChangedIfChanged(() =>
            {
                List<int> updatedLineNumbers =
                this.UpdateTracked(currentSnapshot, ranges);
                List<LineRange> newRanges = ranges;
                IEnumerable<int> addedLineNumbers = this.AddDistinctTrackedNewCodeLinesIfNotExcluded(newRanges, currentSnapshot);
                return updatedLineNumbers.Concat(addedLineNumbers).OrderBy(lineNumber => lineNumber).ToList();
            }, currentSnapshot);

        private List<int> UpdateTracked(
            ITextSnapshot currentSnapshot,
            List<LineRange> ranges
        )
        {
            var removals = new List<ITrackedNewCodeLine>();
            var changedLineNumbers = this.trackedNewCodeLines.SelectMany(
            trackedNewCodeLine => this.UpdateTrackedNewCodeLine(trackedNewCodeLine, currentSnapshot, ranges, removals))
                .Distinct().ToList();
            removals.ForEach(removal => this.trackedNewCodeLines.Remove(removal));
            return changedLineNumbers;
        }

        private IEnumerable<int> UpdateTrackedNewCodeLine(
            ITrackedNewCodeLine trackedNewCodeLine,
            ITextSnapshot currentSnapshot,
            List<LineRange> ranges,
            List<ITrackedNewCodeLine> removals)
        {
            TrackedNewCodeLineUpdate trackedNewCodeLineUpdate = trackedNewCodeLine.Update(currentSnapshot);
            this.RemoveIfTracked(ranges, trackedNewCodeLineUpdate.NewLineNumber);
            return this.ApplyUpdate(trackedNewCodeLineUpdate, trackedNewCodeLine,removals);
        }

        private IEnumerable<int> ApplyUpdate(
            TrackedNewCodeLineUpdate trackedNewCodeLineUpdate,
            ITrackedNewCodeLine trackedNewCodeLine,
            List<ITrackedNewCodeLine> removals)
        {
            bool excluded = this.codeLineExcluder.ExcludeIfNotCode(trackedNewCodeLineUpdate.Text);
            if (excluded)
            {
                removals.Add(trackedNewCodeLine);
                return new int[1] { trackedNewCodeLineUpdate.OldLineNumber };
            }

            return trackedNewCodeLineUpdate.NewLineNumber != trackedNewCodeLineUpdate.OldLineNumber
                ? (new int[2] { trackedNewCodeLineUpdate.OldLineNumber, trackedNewCodeLineUpdate.NewLineNumber })
                : Enumerable.Empty<int>();
        }

        private void RemoveIfTracked(List<LineRange> ranges, int updatedLineNumber)
            => _ = ranges.RemoveAll(spanAndLineRange => spanAndLineRange.StartLineNumber == updatedLineNumber);

        private IEnumerable<int> AddDistinctTrackedNewCodeLinesIfNotExcluded(IEnumerable<LineRange> ranges, ITextSnapshot currentSnapshot)
            => this.GetDistinctStartLineNumbers(ranges)
                    .Where(lineNumber => this.AddTrackedNewCodeLineIfNotExcluded(currentSnapshot, lineNumber));

        private IEnumerable<int> GetDistinctStartLineNumbers(IEnumerable<LineRange> ranges)
            => ranges.Select(spanAndLineNumber => spanAndLineNumber.StartLineNumber).Distinct();

        private bool AddTrackedNewCodeLineIfNotExcluded(ITextSnapshot currentSnapshot, int lineNumber)
            => this.trackedNewCodeLines.AddIfNotNull(
                this.CreateTrackedNewCodeLineIfNotExcluded(currentSnapshot, lineNumber));

        private ITrackedNewCodeLine CreateTrackedNewCodeLineIfNotExcluded(ITextSnapshot currentSnapshot, int lineNumber)
        {
            ITrackedNewCodeLine trackedNewCodeLine = this.CreateTrackedNewCodeLine(currentSnapshot, lineNumber);
            string text = trackedNewCodeLine.GetText(currentSnapshot);
            return this.codeLineExcluder.ExcludeIfNotCode(text) ? null : trackedNewCodeLine;
        }

        #endregion

        private ITrackedNewCodeLine CreateTrackedNewCodeLine(ITextSnapshot currentSnapshot, int lineNumber)
            => this.trackedNewCodeLineFactory.Create(currentSnapshot, SpanTrackingMode.EdgeExclusive, lineNumber);
    }
}
