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
        private readonly ITextInfo _textInfo;
        private readonly string _originalFilePath;
        private readonly ITextBuffer _textBuffer;
        private readonly IBufferLineCoverage _bufferLineCoverage;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDynamicLineAndSnapshotSpansLogic _dynamicLineAndSnapshotSpansLogic;
        private readonly ILineSpanTagger<TTag> _lineSpanTagger;
        private readonly IFileIndicatorVisibility _fileIndicatorVisibility;
        private readonly IDynamicLineFilter _dynamicLineFilter;
        private ICoverageTypeFilter _coverageTypeFilter;
        private bool _isDisplayingIndicators;

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
            this._textInfo = textInfo;
            this._originalFilePath = this._textInfo.FilePath;
            this._textBuffer = textInfo.TextBuffer;
            this._bufferLineCoverage = bufferLineCoverage;
            this._coverageTypeFilter = coverageTypeFilter;
            this._eventAggregator = eventAggregator;
            this._dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            this._lineSpanTagger = lineSpanTagger;
            this._fileIndicatorVisibility = fileIndicatorVisibility;
            dynamicLineFilter.FilterChanged += (_, __) => this.RaiseTagsChanged();
            this._dynamicLineFilter = dynamicLineFilter;
            this._isDisplayingIndicators = fileIndicatorVisibility.IsVisible(textInfo.FilePath);
            fileIndicatorVisibility.VisibilityChanged += this.FileIndicatorVisibility_VisibilityChanged;
            _ = eventAggregator.AddListener(this);
        }

        private void FileIndicatorVisibility_VisibilityChanged(object sender, EventArgs e)
        {
            bool newIsDisplayingIndicators = this._fileIndicatorVisibility.IsVisible(this._textInfo.FilePath);
            bool visibilityChanged = newIsDisplayingIndicators != this._isDisplayingIndicators;
            if (!visibilityChanged)
            {
                return;
            }

            this._isDisplayingIndicators = newIsDisplayingIndicators;
            this.RaiseTagsChanged();
        }

        public bool HasCoverage => this._bufferLineCoverage.HasCoverage;

        public void RaiseTagsChanged() => this.RaiseTagsChangedLinesOrAll();

        private void RaiseTagsChangedLinesOrAll(IEnumerable<int> changedLines = null)
        {
            ITextSnapshot currentSnapshot = this._textBuffer.CurrentSnapshot;
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
            => this._bufferLineCoverage.HasCoverage && !this._coverageTypeFilter.Disabled && this._isDisplayingIndicators;

        private IEnumerable<ITagSpan<TTag>> GetTagsFromCoverageLines(NormalizedSnapshotSpanCollection spans)
        {
            List<IDynamicLineAndSnapshotSpan> dynamicLineAndSnapshotSpans = this._dynamicLineAndSnapshotSpansLogic.Apply(this._bufferLineCoverage, spans);
            return this.GetTags(dynamicLineAndSnapshotSpans);
        }

        private static bool IsNewOrDirty(IDynamicLineAndSnapshotSpan dynamicLineAndSnapshotSpan)
        {
            DynamicCoverageType ct = dynamicLineAndSnapshotSpan.Line.CoverageType;
            return ct == DynamicCoverageType.Dirty || ct == DynamicCoverageType.NewLine;
        }

        private IEnumerable<ITagSpan<TTag>> GetTags(List<IDynamicLineAndSnapshotSpan> dynamicLineAndSnapshotSpans)
        {
            Func<IDynamicLine, bool> fileFilter = dynamicLineAndSnapshotSpans.Any() ?
                this._dynamicLineFilter.GetFileFilter(this._originalFilePath) : (_) => true;
            return dynamicLineAndSnapshotSpans.Where(dynamicLineAndSnapshot
                => this._coverageTypeFilter.Show(dynamicLineAndSnapshot.Line.CoverageType) &&
                (IsNewOrDirty(dynamicLineAndSnapshot) || fileFilter(dynamicLineAndSnapshot.Line))
            ).Select(dynamicLineAndSnapshot => this._lineSpanTagger.GetTagSpan(dynamicLineAndSnapshot));
        }

        public void Dispose()
        {
            _ = this._eventAggregator.RemoveListener(this);
            this._fileIndicatorVisibility.VisibilityChanged -= this.FileIndicatorVisibility_VisibilityChanged;
        }

        public void Handle(CoverageChangedMessage message)
        {
            if (!this.IsOwnChange(message))
            {
                return;
            }

            this.HandleOwnChange(message);
        }

        private bool IsOwnChange(CoverageChangedMessage message) => message.FilePath == this._textInfo.FilePath;

        private void HandleOwnChange(CoverageChangedMessage message) => this.RaiseTagsChangedLinesOrAll(message.ChangedLineNumbers);

        public void Handle(CoverageTypeFilterChangedMessage message)
        {
            if (message.Filter.TypeIdentifier != this._coverageTypeFilter.TypeIdentifier)
            {
                return;
            }

            this._coverageTypeFilter = message.Filter;
            if (this.HasCoverage)
            {
                this.RaiseTagsChanged();
            }
        }
    }
}