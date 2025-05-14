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
        protected readonly IEventAggregator eventAggregator;
        private readonly IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic;
        private readonly ILineSpanTagger<TTag> coverageTagger;
        private readonly IDynamicCoverageManager dynamicCoverageManager;
        private readonly ITextInfoFactory textInfoFactory;
        private readonly IFileExcluder[] fileExcluders;
        private readonly IFileIndicatorVisibility fileIndicatorVisibility;
        private readonly IDynamicLineFilter dynamicLineFilter;
        private TCoverageTypeFilter coverageTypeFilter;

        public CoverageTaggerProvider(
            IEventAggregator eventAggregator,
            IAppOptionsProvider appOptionsProvider,
            IDynamicLineAndSnapshotSpansLogic dynamicLineAndSnapshotSpansLogic,
            ILineSpanTagger<TTag> coverageTagger,
            IDynamicCoverageManager dynamicCoverageManager,
            ITextInfoFactory textInfoFactory,
            IFileExcluder[] fileExcluders,
            IFileIndicatorVisibility fileIndicatorVisibility,
            IDynamicLineFilter dynamicLineFilter
            )
        {
            this.dynamicCoverageManager = dynamicCoverageManager;
            this.textInfoFactory = textInfoFactory;
            this.fileExcluders = fileExcluders;
            this.fileIndicatorVisibility = fileIndicatorVisibility;
            this.dynamicLineFilter = dynamicLineFilter;
            AppOptions appOptions = appOptionsProvider.Get();
            this.coverageTypeFilter = CreateFilter(appOptions);
            appOptionsProvider.OptionsChanged += this.AppOptionsProvider_OptionsChanged;
            this.eventAggregator = eventAggregator;
            this.dynamicLineAndSnapshotSpansLogic = dynamicLineAndSnapshotSpansLogic;
            this.coverageTagger = coverageTagger;
        }

        private static TCoverageTypeFilter CreateFilter(AppOptions appOptions)
        {
            var newCoverageTypeFilter = new TCoverageTypeFilter();
            newCoverageTypeFilter.Initialize(appOptions);
            return newCoverageTypeFilter;
        }

        private void AppOptionsProvider_OptionsChanged(AppOptions appOptions)
        {
            TCoverageTypeFilter newCoverageTypeFilter = CreateFilter(appOptions);
            if (newCoverageTypeFilter.Changed(this.coverageTypeFilter))
            {
                this.coverageTypeFilter = newCoverageTypeFilter;
                var message = new CoverageTypeFilterChangedMessage(newCoverageTypeFilter);
                this.eventAggregator.SendMessage(message);
            }
        }

        private bool ExcludeContentTypeFile(string contentType,string filePath)
        {
            IFileExcluder contentTypeExcluder = this.fileExcluders.FirstOrDefault(fileExcluder => fileExcluder.ContentTypeName == contentType);
            return contentTypeExcluder?.Exclude(filePath) == true;
        }

        public ICoverageTagger<TTag> CreateTagger(ITextView textView, ITextBuffer textBuffer)
        {
            ITextInfo textInfo = this.textInfoFactory.Create(textView, textBuffer);
            string filePath = textInfo.FilePath;
            if (filePath == null || this.ExcludeContentTypeFile(textBuffer.ContentType.TypeName, filePath))
            {
                return null;
            }

            IBufferLineCoverage bufferLineCoverage = this.dynamicCoverageManager.Manage(textInfo);
            return new CoverageTagger<TTag>(
                textInfo,
                bufferLineCoverage,
                this.coverageTypeFilter,
                this.eventAggregator,
                this.dynamicLineAndSnapshotSpansLogic,
                this.coverageTagger,
                this.fileIndicatorVisibility,
                this.dynamicLineFilter
                );
        }
    }
}
