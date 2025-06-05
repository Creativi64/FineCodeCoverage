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
            _textInfo = textInfo;
            _originalFilePath = _textInfo.FilePath;
            _textBuffer = textInfo.TextBuffer;
            _bufferLineCoverage = bufferLineCoverage;
            _coverageTypeFilter = coverageTypeFilter;
            _eventAggregator = eventAggregator;
            _dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            _lineSpanTagger = lineSpanTagger;
            _fileIndicatorVisibility = fileIndicatorVisibility;
            dynamicLineFilter.FilterChanged += (_, __) => RaiseTagsChanged();
            _dynamicLineFilter = dynamicLineFilter;
            _isDisplayingIndicators = fileIndicatorVisibility.IsVisible(textInfo.FilePath);
            fileIndicatorVisibility.VisibilityChanged += FileIndicatorVisibility_VisibilityChanged;
            _ = eventAggregator.AddListener(this);
        }

        private void FileIndicatorVisibility_VisibilityChanged(object sender, EventArgs e)
        {
            bool newIsDisplayingIndicators = _fileIndicatorVisibility.IsVisible(_textInfo.FilePath);
            bool visibilityChanged = newIsDisplayingIndicators != _isDisplayingIndicators;
            if (!visibilityChanged)
            {
                return;
            }

            _isDisplayingIndicators = newIsDisplayingIndicators;
            RaiseTagsChanged();
        }

        public bool HasCoverage => _bufferLineCoverage.HasCoverage;

        public void RaiseTagsChanged() => RaiseTagsChangedLinesOrAll();

        private void RaiseTagsChangedLinesOrAll(IEnumerable<int> changedLines = null)
        {
            ITextSnapshot currentSnapshot = _textBuffer.CurrentSnapshot;
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
            => CanGetTagsFromCoverageLines
                ? GetTagsFromCoverageLines(spans)
                : Enumerable.Empty<ITagSpan<TTag>>();

        private bool CanGetTagsFromCoverageLines
            => _bufferLineCoverage.HasCoverage && !_coverageTypeFilter.Disabled && _isDisplayingIndicators;

        private IEnumerable<ITagSpan<TTag>> GetTagsFromCoverageLines(NormalizedSnapshotSpanCollection spans)
        {
            List<IDynamicLineAndSnapshotSpan> dynamicLineAndSnapshotSpans = _dynamicLineAndSnapshotSpansLogic.Apply(_bufferLineCoverage, spans);
            return GetTags(dynamicLineAndSnapshotSpans);
        }

        private static bool IsNewOrDirty(IDynamicLineAndSnapshotSpan dynamicLineAndSnapshotSpan)
        {
            DynamicCoverageType ct = dynamicLineAndSnapshotSpan.Line.CoverageType;
            return ct == DynamicCoverageType.Dirty || ct == DynamicCoverageType.NewLine;
        }

        private IEnumerable<ITagSpan<TTag>> GetTags(List<IDynamicLineAndSnapshotSpan> dynamicLineAndSnapshotSpans)
        {
            Func<IDynamicLine, bool> fileFilter = dynamicLineAndSnapshotSpans.Count != 0 ?
                _dynamicLineFilter.GetFileFilter(_originalFilePath) : (_) => true;
            return dynamicLineAndSnapshotSpans.Where(dynamicLineAndSnapshot
                => _coverageTypeFilter.Show(dynamicLineAndSnapshot.Line.CoverageType) &&
                (IsNewOrDirty(dynamicLineAndSnapshot) || fileFilter(dynamicLineAndSnapshot.Line))
            ).Select(dynamicLineAndSnapshot => _lineSpanTagger.GetTagSpan(dynamicLineAndSnapshot));
        }

        public void Dispose()
        {
            _ = _eventAggregator.RemoveListener(this);
            _fileIndicatorVisibility.VisibilityChanged -= FileIndicatorVisibility_VisibilityChanged;
        }

        public void Handle(CoverageChangedMessage message)
        {
            if (!IsOwnChange(message))
            {
                return;
            }

            HandleOwnChange(message);
        }

        private bool IsOwnChange(CoverageChangedMessage message) => message.FilePath == _textInfo.FilePath;

        private void HandleOwnChange(CoverageChangedMessage message) => RaiseTagsChangedLinesOrAll(message.ChangedLineNumbers);

        public void Handle(CoverageTypeFilterChangedMessage message)
        {
            if (message.Filter.TypeIdentifier != _coverageTypeFilter.TypeIdentifier)
            {
                return;
            }

            _coverageTypeFilter = message.Filter;
            if (!HasCoverage)
            {
                return;
            }

            RaiseTagsChanged();
        }
    }
}
