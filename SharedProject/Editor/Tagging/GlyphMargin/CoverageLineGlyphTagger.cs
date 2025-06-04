using System;
using System.Collections.Generic;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Editor.Tagging.Base;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace FineCodeCoverage.Editor.Tagging.GlyphMargin
{
    internal class CoverageLineGlyphTagger : ITagger<CoverageLineGlyphTag>, IDisposable, IListener<CoverageColoursChangedMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoverageTagger<CoverageLineGlyphTag> _coverageTagger;

        public CoverageLineGlyphTagger(
            IEventAggregator eventAggregator,
            ICoverageTagger<CoverageLineGlyphTag> coverageTagger
        )
        {
            ThrowIf.Null(coverageTagger, nameof(coverageTagger));
            _ = eventAggregator.AddListener(this);
            this._eventAggregator = eventAggregator;
            this._coverageTagger = coverageTagger;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add => this._coverageTagger.TagsChanged += value;
            remove => this._coverageTagger.TagsChanged -= value;
        }

        public void Dispose()
        {
            this._coverageTagger.Dispose();
            _ = this._eventAggregator.RemoveListener(this);
        }

        public IEnumerable<ITagSpan<CoverageLineGlyphTag>> GetTags(NormalizedSnapshotSpanCollection spans)
            => this._coverageTagger.GetTags(spans);

        public void Handle(CoverageColoursChangedMessage message)
        {
            if (!this._coverageTagger.HasCoverage)
            {
                return;
            }

            this._coverageTagger.RaiseTagsChanged();
        }
    }
}