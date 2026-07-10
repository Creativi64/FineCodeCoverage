using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Editor.DynamicCoverage.Messages;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Utilities.Events;

namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IDynamicCoverageManager))]
    internal sealed class DynamicCoverageManager :
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
            _bufferLineCoverageFactory = bufferLineCoverageFactory;
            _reportFileLineCoverageFactory = reportFileLineCoverageFactory;
            _dateTimeService = dateTimeService;
            _ = eventAggregator.AddListener(this);
            _eventAggregator = eventAggregator;
            _trackedLinesFactory = trackedLinesFactory;
        }

        public void Handle(NewReportMessage message)
        {
            if (message.Report == null)
            {
                return;
            }

            IFileLineCoverage fileLineCoverage = _reportFileLineCoverageFactory.Create(message.Report.Assemblies);
            _lastCoverage = new LastCoverage(fileLineCoverage, _lastTestExecutionStartingDate);
            _eventAggregator.SendMessage(new NewCoverageLinesMessage(fileLineCoverage));
        }

        public void Handle(TestExecutionStartingMessage message) => _lastTestExecutionStartingDate = _dateTimeService.Now;

        public IBufferLineCoverage Manage(ITextInfo textInfo)
            => textInfo.TextBuffer.Properties.GetOrCreateSingletonProperty(
                () =>
                {
                    IBufferLineCoverage bufferLineCoverage = _bufferLineCoverageFactory.Create(textInfo, _eventAggregator, _trackedLinesFactory);
                    if (_lastCoverage != null)
                    {
                        bufferLineCoverage.SetLastCoverage(_lastCoverage);
                    }

                    return bufferLineCoverage;
                });
    }
}
