using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IDynamicCoverageManager))]
    internal class DynamicCoverageManager :
        IDynamicCoverageManager,
        IListener<TestExecutionStartingMessage>,
        IListener<NewReportMessage>,
        IInitializable
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ITrackedLinesFactory trackedLinesFactory;
        private readonly IBufferLineCoverageFactory bufferLineCoverageFactory;
        private readonly IReportFileLineCoverageFactory reportFileLineCoverageFactory;
        private readonly IDateTimeService dateTimeService;
        private LastCoverage lastCoverage;
        private DateTime lastTestExecutionStartingDate;

        [ImportingConstructor]
        public DynamicCoverageManager(
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IBufferLineCoverageFactory bufferLineCoverageFactory,
            IReportFileLineCoverageFactory reportFileLineCoverageFactory,
            IDateTimeService dateTimeService)
        {
            this.bufferLineCoverageFactory = bufferLineCoverageFactory;
            this.reportFileLineCoverageFactory = reportFileLineCoverageFactory;
            this.dateTimeService = dateTimeService;
            _ = eventAggregator.AddListener(this);
            this.eventAggregator = eventAggregator;
            this.trackedLinesFactory = trackedLinesFactory;
        }

        public void Handle(NewReportMessage message) {
            IFileLineCoverage fileLineCoverage = this.reportFileLineCoverageFactory.Create(message.Report.Assemblies);
            this.lastCoverage = new LastCoverage(fileLineCoverage, this.lastTestExecutionStartingDate);
            this.eventAggregator.SendMessage(new NewCoverageLinesMessage(fileLineCoverage));
        }

        public void Handle(TestExecutionStartingMessage message) => this.lastTestExecutionStartingDate = this.dateTimeService.Now;

        public IBufferLineCoverage Manage(ITextInfo textInfo)
            => textInfo.TextBuffer.Properties.GetOrCreateSingletonProperty(
                () => this.bufferLineCoverageFactory.Create(this.lastCoverage, textInfo, this.eventAggregator, this.trackedLinesFactory)
            );
    }
}
