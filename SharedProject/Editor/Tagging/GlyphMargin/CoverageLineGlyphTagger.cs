using System;
using System.Collections.Generic;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Editor.Tagging.Base;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace FineCodeCoverage.Editor.Tagging.GlyphMargin
{
    internal sealed class CoverageLineGlyphTagger : ITagger<CoverageLineGlyphTag>, IDisposable, IListener<CoverageColoursChangedMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoverageTagger<CoverageLineGlyphTag> _coverageTagger;

        public CoverageLineGlyphTagger(
            IEventAggregator eventAggregator,
            ICoverageTagger<CoverageLineGlyphTag> coverageTagger)
        {
            ThrowIf.Null(coverageTagger, nameof(coverageTagger));
            _ = eventAggregator.AddListener(this);
            _eventAggregator = eventAggregator;
            _coverageTagger = coverageTagger;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add => _coverageTagger.TagsChanged += value;
            remove => _coverageTagger.TagsChanged -= value;
        }

        public void Dispose()
        {
            _coverageTagger.Dispose();
            _ = _eventAggregator.RemoveListener(this);
        }

        public IEnumerable<ITagSpan<CoverageLineGlyphTag>> GetTags(NormalizedSnapshotSpanCollection spans)
            => _coverageTagger.GetTags(spans);

        public void Handle(CoverageColoursChangedMessage message)
        {
            if (!_coverageTagger.HasCoverage)
            {
                return;
            }

            _coverageTagger.RaiseTagsChanged();
        }
    }
}
