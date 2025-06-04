using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class NewCodeTracker : INewCodeTracker
    {
        private readonly List<ITrackedNewCodeLine> _trackedNewCodeLines = new List<ITrackedNewCodeLine>();
        private readonly ITrackedNewCodeLineFactory _trackedNewCodeLineFactory;
        private readonly ILineExcluder _codeLineExcluder;
        private readonly ITextInfoFactory _textInfoFactory;

        public NewCodeTracker(
            ITrackedNewCodeLineFactory trackedNewCodeLineFactory,
            ILineExcluder codeLineExcluder,
            ITextInfoFactory textInfoFactory
            )
        {
            _trackedNewCodeLineFactory = trackedNewCodeLineFactory;
            _codeLineExcluder = codeLineExcluder;
            _textInfoFactory = textInfoFactory;
        }

        public event EventHandler<HasNewCodeChangedEventArgs> HasNewCodeChanged;

        public IEnumerable<IDynamicLine> Lines
            => _trackedNewCodeLines.OrderBy(l => l.Line.LineNumber).Select(l => l.Line);

        private void OnHasNewCodeChanged(bool hasNewCode, ITextSnapshot textSnapshot)
            => HasNewCodeChanged?.Invoke(
                this,
                new HasNewCodeChangedEventArgs(
                    _textInfoFactory.GetFilePath(textSnapshot.TextBuffer), hasNewCode
                )
            );

        private T OnHasNewCodeChangedIfChanged<T>(Func<T> action, ITextSnapshot currentSnapshot)
        {
            bool hasTracked = _trackedNewCodeLines.Count != 0;
            T t = action();
            bool newHasTracked = _trackedNewCodeLines.Count != 0;
            if (newHasTracked != hasTracked)
            {
                OnHasNewCodeChanged(newHasTracked, currentSnapshot);
            }

            return t;
        }

        #region CodeSpanRange

        public IEnumerable<int> GetChangedLineNumbers(ITextSnapshot textSnapshot, List<CodeSpanRange> codeSpanRanges)
    => OnHasNewCodeChangedIfChanged(() =>
    {
        List<int> rangeStartLineNumbers = codeSpanRanges.ConvertAll(newCodeCodeRange => newCodeCodeRange.StartLine);
        List<int> removed = RemoveAndReduceByDynamicLineNumbers(rangeStartLineNumbers);
        List<int> newRangeStartLineNumbers = rangeStartLineNumbers;
        CreateTrackedNewCodeLinesFromRangeStartLineNumbers(newRangeStartLineNumbers, textSnapshot);
        return removed.Concat(newRangeStartLineNumbers).OrderBy(lineNumber => lineNumber).ToList();
    }, textSnapshot);

        private List<int> RemoveAndReduceByDynamicLineNumbers(List<int> rangeStartLineNumbers)
        {
            var removals = _trackedNewCodeLines.Where(
                trackedNewCodeLine => !rangeStartLineNumbers.Remove(trackedNewCodeLine.Line.LineNumber)).ToList();

            removals.ForEach(removal => _trackedNewCodeLines.Remove(removal));
            return removals.ConvertAll(removal => removal.Line.LineNumber);
        }

        private void CreateTrackedNewCodeLinesFromRangeStartLineNumbers(IEnumerable<int> rangeStartLineNumbers, ITextSnapshot currentSnapshot)
            => _trackedNewCodeLines.AddRange(
                rangeStartLineNumbers.Select(lineNumber => CreateTrackedNewCodeLine(currentSnapshot, lineNumber))
            );

        #endregion

        #region LineRange

        public IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<LineRange> ranges)
            => OnHasNewCodeChangedIfChanged(() =>
            {
                List<int> updatedLineNumbers =
                UpdateTracked(currentSnapshot, ranges);
                List<LineRange> newRanges = ranges;
                IEnumerable<int> addedLineNumbers = AddDistinctTrackedNewCodeLinesIfNotExcluded(newRanges, currentSnapshot);
                return updatedLineNumbers.Concat(addedLineNumbers).OrderBy(lineNumber => lineNumber).ToList();
            }, currentSnapshot);

        private List<int> UpdateTracked(
            ITextSnapshot currentSnapshot,
            List<LineRange> ranges
        )
        {
            var removals = new List<ITrackedNewCodeLine>();
            var changedLineNumbers = _trackedNewCodeLines.SelectMany(
            trackedNewCodeLine => UpdateTrackedNewCodeLine(trackedNewCodeLine, currentSnapshot, ranges, removals))
                .Distinct().ToList();
            removals.ForEach(removal => _trackedNewCodeLines.Remove(removal));
            return changedLineNumbers;
        }

        private IEnumerable<int> UpdateTrackedNewCodeLine(
            ITrackedNewCodeLine trackedNewCodeLine,
            ITextSnapshot currentSnapshot,
            List<LineRange> ranges,
            List<ITrackedNewCodeLine> removals)
        {
            TrackedNewCodeLineUpdate trackedNewCodeLineUpdate = trackedNewCodeLine.Update(currentSnapshot);
            RemoveIfTracked(ranges, trackedNewCodeLineUpdate.NewLineNumber);
            return ApplyUpdate(trackedNewCodeLineUpdate, trackedNewCodeLine, removals);
        }

        private IEnumerable<int> ApplyUpdate(
            TrackedNewCodeLineUpdate trackedNewCodeLineUpdate,
            ITrackedNewCodeLine trackedNewCodeLine,
            List<ITrackedNewCodeLine> removals)
        {
            bool excluded = _codeLineExcluder.ExcludeIfNotCode(trackedNewCodeLineUpdate.Text);
            if (excluded)
            {
                removals.Add(trackedNewCodeLine);
                return new int[1] { trackedNewCodeLineUpdate.OldLineNumber };
            }

            return trackedNewCodeLineUpdate.NewLineNumber != trackedNewCodeLineUpdate.OldLineNumber
                ? (new int[2] { trackedNewCodeLineUpdate.OldLineNumber, trackedNewCodeLineUpdate.NewLineNumber })
                : Enumerable.Empty<int>();
        }

        private static void RemoveIfTracked(List<LineRange> ranges, int updatedLineNumber)
            => _ = ranges.RemoveAll(spanAndLineRange => spanAndLineRange.StartLineNumber == updatedLineNumber);

        private IEnumerable<int> AddDistinctTrackedNewCodeLinesIfNotExcluded(IEnumerable<LineRange> ranges, ITextSnapshot currentSnapshot)
            => GetDistinctStartLineNumbers(ranges)
                    .Where(lineNumber => AddTrackedNewCodeLineIfNotExcluded(currentSnapshot, lineNumber));

        private static IEnumerable<int> GetDistinctStartLineNumbers(IEnumerable<LineRange> ranges)
            => ranges.Select(spanAndLineNumber => spanAndLineNumber.StartLineNumber).Distinct();

        private bool AddTrackedNewCodeLineIfNotExcluded(ITextSnapshot currentSnapshot, int lineNumber)
            => _trackedNewCodeLines.AddIfNotNull(
                CreateTrackedNewCodeLineIfNotExcluded(currentSnapshot, lineNumber));

        private ITrackedNewCodeLine CreateTrackedNewCodeLineIfNotExcluded(ITextSnapshot currentSnapshot, int lineNumber)
        {
            ITrackedNewCodeLine trackedNewCodeLine = CreateTrackedNewCodeLine(currentSnapshot, lineNumber);
            string text = trackedNewCodeLine.GetText(currentSnapshot);
            return _codeLineExcluder.ExcludeIfNotCode(text) ? null : trackedNewCodeLine;
        }

        #endregion

        private ITrackedNewCodeLine CreateTrackedNewCodeLine(ITextSnapshot currentSnapshot, int lineNumber)
            => _trackedNewCodeLineFactory.Create(currentSnapshot, SpanTrackingMode.EdgeExclusive, lineNumber);
    }
}
