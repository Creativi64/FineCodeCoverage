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

        public IEnumerable<int> GetChangedLineNumbers(
            ITextSnapshot currentSnapshot,
            List<LineRange> potentialNewLines,
            IEnumerable<CodeSpanRange> newCodeCodeRanges
        ) => this.OnHasNewCodeChangedIfChanged(
            () => this.GetChangedLineNumbersActual(currentSnapshot, potentialNewLines, newCodeCodeRanges), currentSnapshot);

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

        private IEnumerable<int> GetChangedLineNumbersActual(
            ITextSnapshot currentSnapshot,
            List<LineRange> potentialNewLines,
            IEnumerable<CodeSpanRange> newCodeCodeRanges
        ) => newCodeCodeRanges != null
                    ? this.GetChangedLineNumbers(newCodeCodeRanges, currentSnapshot)
                : this.GetChangedLineNumbers(potentialNewLines, currentSnapshot);

        #region NewCodeCodeRanges

        private List<int> GetChangedLineNumbers(IEnumerable<CodeSpanRange> newCodeCodeRanges, ITextSnapshot textSnapshot)
        {
            var startLineNumbers = newCodeCodeRanges.Select(newCodeCodeRange => newCodeCodeRange.StartLine).ToList();
            IEnumerable<int> removed = this.RemoveAndReduceByLineNumbers(startLineNumbers);
            this.CreateTrackedNewCodeLines(startLineNumbers, textSnapshot);
            return removed.Concat(startLineNumbers).ToList();
        }

        private IEnumerable<int> RemoveAndReduceByLineNumbers(List<int> startLineNumbers)
        {
            var removals = this.trackedNewCodeLines.Where(
                trackedNewCodeLine => !startLineNumbers.Remove(trackedNewCodeLine.Line.Number)).ToList();

            removals.ForEach(removal => this.trackedNewCodeLines.Remove(removal));
            return removals.Select(removal => removal.Line.Number);
        }

        private void CreateTrackedNewCodeLines(IEnumerable<int> lineNumbers, ITextSnapshot currentSnapshot)
            => this.trackedNewCodeLines.AddRange(
                lineNumbers.Select(lineNumber => this.CreateTrackedNewCodeLine(currentSnapshot, lineNumber))
            );

        #endregion

        #region SpanAndLineRanges
        private List<int> GetChangedLineNumbers(List<LineRange> potentialNewLines, ITextSnapshot currentSnapshot)
        {
            IEnumerable<int> updatedLineNumbers =
                this.UpdateTracked(currentSnapshot, potentialNewLines);
            List<LineRange> newLines = potentialNewLines;
            IEnumerable<int> addedLineNumbers = this.AddDistinctTrackedNewCodeLinesIfNotExcluded(newLines, currentSnapshot);
            return updatedLineNumbers.Concat(addedLineNumbers).ToList();
        }

        private IEnumerable<int> UpdateTracked(
            ITextSnapshot currentSnapshot,
            List<LineRange> potentialNewLines
        ) => this.trackedNewCodeLines.SelectMany(
            trackedNewCodeLine => this.UpdateTrackedNewCodeLine(trackedNewCodeLine, currentSnapshot, potentialNewLines)).Distinct();

        private IEnumerable<int> UpdateTrackedNewCodeLine(
            ITrackedNewCodeLine trackedNewCodeLine,
            ITextSnapshot currentSnapshot,
            List<LineRange> potentialNewLines
        )
        {
            TrackedNewCodeLineUpdate trackedNewCodeLineUpdate = trackedNewCodeLine.Update(currentSnapshot);
            this.RemoveIfTracked(potentialNewLines, trackedNewCodeLineUpdate.NewLineNumber);
            return this.ApplyUpdate(trackedNewCodeLineUpdate, trackedNewCodeLine);
        }

        private IEnumerable<int> ApplyUpdate(TrackedNewCodeLineUpdate trackedNewCodeLineUpdate, ITrackedNewCodeLine trackedNewCodeLine)
        {
            bool excluded = this.codeLineExcluder.ExcludeIfNotCode(trackedNewCodeLineUpdate.Text);
            if (excluded)
            {
                _ = this.trackedNewCodeLines.Remove(trackedNewCodeLine);
                return new int[1] { trackedNewCodeLineUpdate.OldLineNumber };
            }

            return trackedNewCodeLineUpdate.NewLineNumber != trackedNewCodeLineUpdate.OldLineNumber
                ? (new int[2] { trackedNewCodeLineUpdate.OldLineNumber, trackedNewCodeLineUpdate.NewLineNumber })
                : Enumerable.Empty<int>();
        }

        private void RemoveIfTracked(List<LineRange> potentialNewLines, int updatedLineNumber)
            => _ = potentialNewLines.RemoveAll(spanAndLineRange => spanAndLineRange.StartLineNumber == updatedLineNumber);

        private IEnumerable<int> AddDistinctTrackedNewCodeLinesIfNotExcluded(IEnumerable<LineRange> newLines, ITextSnapshot currentSnapshot)
            => this.GetDistinctStartLineNumbers(newLines)
                    .Where(lineNumber => this.AddTrackedNewCodeLineIfNotExcluded(currentSnapshot, lineNumber));

        private IEnumerable<int> GetDistinctStartLineNumbers(IEnumerable<LineRange> potentialNewLines)
            => potentialNewLines.Select(spanAndLineNumber => spanAndLineNumber.StartLineNumber).Distinct();

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
