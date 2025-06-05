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
        private readonly IEventAggregator _eventAggregator;
        private TCoverageTypeFilter _coverageTypeFilter;

        public CoverageTaggerProvider(
            IEventAggregator eventAggregator,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic,
            ILineSpanTagger<TTag> coverageTagger,
            IDynamicCoverageManager dynamicCoverageManager,
            ITextInfoFactory textInfoFactory,
            IFileExcluder[] fileExcluders,
            IFileIndicatorVisibility fileIndicatorVisibility,
            IDynamicLineFilter dynamicLineFilter)
        {
            _dynamicCoverageManager = dynamicCoverageManager;
            _textInfoFactory = textInfoFactory;
            _fileExcluders = fileExcluders;
            _fileIndicatorVisibility = fileIndicatorVisibility;
            _dynamicLineFilter = dynamicLineFilter;
            EditorCoverageColouringOptions appOptions = editorCoverageColouringOptionsProvider.Get();
            _coverageTypeFilter = CreateFilter(appOptions);
            editorCoverageColouringOptionsProvider.OptionsChanged += EditorCoverageColouringOptionsProvider_OptionsChanged;
            _eventAggregator = eventAggregator;
            _dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            _coverageTagger = coverageTagger;
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
            if (!newCoverageTypeFilter.Changed(_coverageTypeFilter))
            {
                return;
            }

            _coverageTypeFilter = newCoverageTypeFilter;
            var message = new CoverageTypeFilterChangedMessage(newCoverageTypeFilter);
            _eventAggregator.SendMessage(message);
        }

        private bool ExcludeContentTypeFile(string contentType, string filePath)
        {
            IFileExcluder contentTypeExcluder = _fileExcluders.FirstOrDefault(fileExcluder => fileExcluder.ContentTypeName == contentType);
            return contentTypeExcluder?.Exclude(filePath) == true;
        }

        public ICoverageTagger<TTag> CreateTagger(ITextView textView, ITextBuffer textBuffer)
        {
            ITextInfo textInfo = _textInfoFactory.Create(textView, textBuffer);
            string filePath = textInfo.FilePath;
            if (filePath == null || ExcludeContentTypeFile(textBuffer.ContentType.TypeName, filePath))
            {
                return null;
            }

            IBufferLineCoverage bufferLineCoverage = _dynamicCoverageManager.Manage(textInfo);
            return new CoverageTagger<TTag>(
                textInfo,
                bufferLineCoverage,
                _coverageTypeFilter,
                _eventAggregator,
                _dynamicLineAndSnapshotSpansLogic,
                _coverageTagger,
                _fileIndicatorVisibility,
                _dynamicLineFilter);
        }
    }
}
