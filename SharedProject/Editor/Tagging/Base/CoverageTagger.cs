using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.IndicatorVisibility;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal class CoverageTagger<TTag> :
        ICoverageTagger<TTag>,
        IListener<CoverageTypeFilterChangedMessage>,
        IListener<CoverageChangedMessage>
        where TTag : ITag

    {
        private readonly ITextInfo textInfo;
        private readonly string originalFilePath;
        private readonly ITextBuffer textBuffer;
        private readonly IBufferLineCoverage bufferLineCoverage;
        private ICoverageTypeFilter coverageTypeFilter;
        private readonly IEventAggregator eventAggregator;
        private readonly IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic;
        private readonly ILineSpanTagger<TTag> lineSpanTagger;
        private readonly IFileIndicatorVisibility fileIndicatorVisibility;
        private readonly IDynamicLineFilter dynamicLineFilter;
        private bool isDisplayingIndicators;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public CoverageTagger(
            ITextInfo textInfo,
            IBufferLineCoverage bufferLineCoverage,
            ICoverageTypeFilter coverageTypeFilter,
            IEventAggregator eventAggregator,
            IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic,
            ILineSpanTagger<TTag> lineSpanTagger,
            IFileIndicatorVisibility fileIndicatorVisibility,
            IDynamicLineFilter dynamicLineFilter
            )
        {
            ThrowIf.Null(textInfo, nameof(textInfo));
            ThrowIf.Null(coverageTypeFilter, nameof(coverageTypeFilter));
            ThrowIf.Null(eventAggregator, nameof(eventAggregator));
            ThrowIf.Null(dynamicLineAndSnapshotSpansLogic, nameof(dynamicLineAndSnapshotSpansLogic));
            ThrowIf.Null(lineSpanTagger, nameof(lineSpanTagger));
            this.textInfo = textInfo;
            this.originalFilePath = this.textInfo.FilePath;
            this.textBuffer = textInfo.TextBuffer;
            this.bufferLineCoverage = bufferLineCoverage;
            this.coverageTypeFilter = coverageTypeFilter;
            this.eventAggregator = eventAggregator;
            this.dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            this.lineSpanTagger = lineSpanTagger;
            this.fileIndicatorVisibility = fileIndicatorVisibility;
            dynamicLineFilter.FilterChanged += (_, __) => this.RaiseTagsChanged();
            this.dynamicLineFilter = dynamicLineFilter;
            this.isDisplayingIndicators = fileIndicatorVisibility.IsVisible(textInfo.FilePath);
            fileIndicatorVisibility.VisibilityChanged += this.FileIndicatorVisibility_VisibilityChanged;
            _ = eventAggregator.AddListener(this);
        }

        private void FileIndicatorVisibility_VisibilityChanged(object sender, EventArgs e)
        {
            bool newIsDisplayingIndicators = this.fileIndicatorVisibility.IsVisible(this.textInfo.FilePath);
            bool visibilityChanged = newIsDisplayingIndicators != this.isDisplayingIndicators;
            if (visibilityChanged)
            {
                this.isDisplayingIndicators = newIsDisplayingIndicators;
                this.RaiseTagsChanged();
            }
        }

        public bool HasCoverage => this.bufferLineCoverage.HasCoverage;

        public void RaiseTagsChanged() => this.RaiseTagsChangedLinesOrAll();

        private void RaiseTagsChangedLinesOrAll(IEnumerable<int> changedLines = null)
        {
            ITextSnapshot currentSnapshot = this.textBuffer.CurrentSnapshot;
            SnapshotSpan snapshotSpan;
            if (changedLines != null)
            {
                Span span = changedLines.Select(changedLine => currentSnapshot.GetLineFromLineNumber(changedLine).Extent.Span)
                    .Aggregate((acc, next) => Span.FromBounds(Math.Min(acc.Start, next.Start), Math.Max(acc.End, next.End)));
                snapshotSpan = new SnapshotSpan(currentSnapshot, span);
            }
            else
            {
                snapshotSpan = new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length);
            }

            var spanEventArgs = new SnapshotSpanEventArgs(snapshotSpan);
            TagsChanged?.Invoke(this, spanEventArgs);
        }

        public IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans)
            => this.CanGetTagsFromCoverageLines
                ? this.GetTagsFromCoverageLines(spans)
                : Enumerable.Empty<ITagSpan<TTag>>();

        private bool CanGetTagsFromCoverageLines
            => this.bufferLineCoverage.HasCoverage && !this.coverageTypeFilter.Disabled && this.isDisplayingIndicators;

        private IEnumerable<ITagSpan<TTag>> GetTagsFromCoverageLines(NormalizedSnapshotSpanCollection spans)
        {
            List<IDynamicLineAndSnapshotSpan> dynamicLineAndSnapshotSpans = this.dynamicLineAndSnapshotSpansLogic.Apply(this.bufferLineCoverage, spans);
            return this.GetTags(dynamicLineAndSnapshotSpans);
        }

        private bool IsNewOrDirty(IDynamicLineAndSnapshotSpan dynamicLineAndSnapshotSpan)
        {
            DynamicCoverageType ct = dynamicLineAndSnapshotSpan.Line.CoverageType;
            return ct == DynamicCoverageType.Dirty || ct == DynamicCoverageType.NewLine;
        }

        private IEnumerable<ITagSpan<TTag>> GetTags(List<IDynamicLineAndSnapshotSpan> dynamicLineAndSnapshotSpans)
        {
            Func<IDynamicLine, bool> fileFilter = dynamicLineAndSnapshotSpans.Any() ?
                this.dynamicLineFilter.GetFileFilter(this.originalFilePath) : (_) => true;
            return dynamicLineAndSnapshotSpans.Where(dynamicLineAndSnapshot
                => this.coverageTypeFilter.Show(dynamicLineAndSnapshot.Line.CoverageType) &&
                (this.IsNewOrDirty(dynamicLineAndSnapshot) || fileFilter(dynamicLineAndSnapshot.Line))
            ).Select(dynamicLineAndSnapshot => this.lineSpanTagger.GetTagSpan(dynamicLineAndSnapshot));
        }

        public void Dispose()
        {
            _ = this.eventAggregator.RemoveListener(this);
            this.fileIndicatorVisibility.VisibilityChanged -= this.FileIndicatorVisibility_VisibilityChanged;
        }

        public void Handle(CoverageChangedMessage message)
        {
            if (this.IsOwnChange(message))
            {
                this.HandleOwnChange(message);
            }
        }

        private bool IsOwnChange(CoverageChangedMessage message) => message.FilePath == this.textInfo.FilePath;

        private void HandleOwnChange(CoverageChangedMessage message) => this.RaiseTagsChangedLinesOrAll(message.ChangedLineNumbers);

        public void Handle(CoverageTypeFilterChangedMessage message)
        {
            if (message.Filter.TypeIdentifier == this.coverageTypeFilter.TypeIdentifier)
            {
                this.coverageTypeFilter = message.Filter;
                if (this.HasCoverage)
                {
                    this.RaiseTagsChanged();
                }
            }
        }
    }
}
