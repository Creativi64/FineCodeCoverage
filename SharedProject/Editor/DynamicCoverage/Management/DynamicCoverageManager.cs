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
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackedLinesFactory _trackedLinesFactory;
        private readonly IBufferLineCoverageFactory _bufferLineCoverageFactory;
        private readonly IReportFileLineCoverageFactory _reportFileLineCoverageFactory;
        private readonly IDateTimeService _dateTimeService;
        private LastCoverage _lastCoverage;
        private DateTime _lastTestExecutionStartingDate;

        [ImportingConstructor]
        public DynamicCoverageManager(
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IBufferLineCoverageFactory bufferLineCoverageFactory,
            IReportFileLineCoverageFactory reportFileLineCoverageFactory,
            IDateTimeService dateTimeService)
        {
            this._bufferLineCoverageFactory = bufferLineCoverageFactory;
            this._reportFileLineCoverageFactory = reportFileLineCoverageFactory;
            this._dateTimeService = dateTimeService;
            _ = eventAggregator.AddListener(this);
            this._eventAggregator = eventAggregator;
            this._trackedLinesFactory = trackedLinesFactory;
        }

        public void Handle(NewReportMessage message)
        {
            IFileLineCoverage fileLineCoverage = this._reportFileLineCoverageFactory.Create(message.Report.Assemblies);
            this._lastCoverage = new LastCoverage(fileLineCoverage, this._lastTestExecutionStartingDate);
            this._eventAggregator.SendMessage(new NewCoverageLinesMessage(fileLineCoverage));
        }

        public void Handle(TestExecutionStartingMessage message) => this._lastTestExecutionStartingDate = this._dateTimeService.Now;

        public IBufferLineCoverage Manage(ITextInfo textInfo)
            => textInfo.TextBuffer.Properties.GetOrCreateSingletonProperty(
                () =>
                {
                    IBufferLineCoverage bufferLineCoverage = this._bufferLineCoverageFactory.Create(textInfo, this._eventAggregator, this._trackedLinesFactory);
                    if (this._lastCoverage != null)
                    {
                        bufferLineCoverage.SetLastCoverage(this._lastCoverage);
                    }

                    return bufferLineCoverage;
                }
            );
    }
}