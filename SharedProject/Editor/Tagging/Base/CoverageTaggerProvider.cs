using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.IndicatorVisibility;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal class CoverageTaggerProvider<TCoverageTypeFilter, TTag> : ICoverageTaggerProvider<TTag>
        where TCoverageTypeFilter : ICoverageTypeFilter, new()
        where TTag : ITag
    {
        private readonly IDynamicLineAndSnapshotSpansLogic _dynamicLineAndSnapshotSpansLogic;
        private readonly ILineSpanTagger<TTag> _coverageTagger;
        private readonly IDynamicCoverageManager _dynamicCoverageManager;
        private readonly ITextInfoFactory _textInfoFactory;
        private readonly IFileExcluder[] _fileExcluders;
        private readonly IFileIndicatorVisibility _fileIndicatorVisibility;
        private readonly IDynamicLineFilter _dynamicLineFilter;
        private TCoverageTypeFilter _coverageTypeFilter;
        protected readonly IEventAggregator eventAggregator;

        public CoverageTaggerProvider(
            IEventAggregator eventAggregator,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic,
            ILineSpanTagger<TTag> coverageTagger,
            IDynamicCoverageManager dynamicCoverageManager,
            ITextInfoFactory textInfoFactory,
            IFileExcluder[] fileExcluders,
            IFileIndicatorVisibility fileIndicatorVisibility,
            IDynamicLineFilter dynamicLineFilter
            )
        {
            this._dynamicCoverageManager = dynamicCoverageManager;
            this._textInfoFactory = textInfoFactory;
            this._fileExcluders = fileExcluders;
            this._fileIndicatorVisibility = fileIndicatorVisibility;
            this._dynamicLineFilter = dynamicLineFilter;
            EditorCoverageColouringOptions appOptions = editorCoverageColouringOptionsProvider.Get();
            this._coverageTypeFilter = CreateFilter(appOptions);
            editorCoverageColouringOptionsProvider.OptionsChanged += this.EditorCoverageColouringOptionsProvider_OptionsChanged;
            this.eventAggregator = eventAggregator;
            this._dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            this._coverageTagger = coverageTagger;
        }

        private static TCoverageTypeFilter CreateFilter(EditorCoverageColouringOptions appOptions)
        {
            var newCoverageTypeFilter = new TCoverageTypeFilter();
            newCoverageTypeFilter.Initialize(appOptions);
            return newCoverageTypeFilter;
        }

        private void EditorCoverageColouringOptionsProvider_OptionsChanged(EditorCoverageColouringOptions appOptions)
        {
            TCoverageTypeFilter newCoverageTypeFilter = CreateFilter(appOptions);
            if (newCoverageTypeFilter.Changed(this._coverageTypeFilter))
            {
                this._coverageTypeFilter = newCoverageTypeFilter;
                var message = new CoverageTypeFilterChangedMessage(newCoverageTypeFilter);
                this.eventAggregator.SendMessage(message);
            }
        }

        private bool ExcludeContentTypeFile(string contentType, string filePath)
        {
            IFileExcluder contentTypeExcluder = this._fileExcluders.FirstOrDefault(fileExcluder => fileExcluder.ContentTypeName == contentType);
            return contentTypeExcluder?.Exclude(filePath) == true;
        }

        public ICoverageTagger<TTag> CreateTagger(ITextView textView, ITextBuffer textBuffer)
        {
            ITextInfo textInfo = this._textInfoFactory.Create(textView, textBuffer);
            string filePath = textInfo.FilePath;
            if (filePath == null || this.ExcludeContentTypeFile(textBuffer.ContentType.TypeName, filePath))
            {
                return null;
            }

            IBufferLineCoverage bufferLineCoverage = this._dynamicCoverageManager.Manage(textInfo);
            return new CoverageTagger<TTag>(
                textInfo,
                bufferLineCoverage,
                this._coverageTypeFilter,
                this.eventAggregator,
                this._dynamicLineAndSnapshotSpansLogic,
                this._coverageTagger,
                this._fileIndicatorVisibility,
                this._dynamicLineFilter
                );
        }
    }
}