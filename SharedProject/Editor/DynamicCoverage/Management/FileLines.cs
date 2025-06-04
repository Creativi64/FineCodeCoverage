using System;
using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class FileLines : IFileLines
    {
        private class TrackedLinesState
        {
            private readonly IDateTimeService _dateTimeService;
            private DateTime _lastTracked;

            public TrackedLinesState(ITrackedLines trackedLines, IDateTimeService dateTimeService)
            {
                TrackedLines = trackedLines;
                _dateTimeService = dateTimeService;
                _lastTracked = _dateTimeService.Now;
            }
            public ITrackedLines TrackedLines { get; }

            internal bool IsOutOfDate(DateTime lastWriteTime) => lastWriteTime > _lastTracked;
            internal void TextViewClosed() => _lastTracked = _dateTimeService.Now;
        }

        private TrackedLinesState _trackedLinesState;
        private readonly IDateTimeService _dateTimeService;

        public FileLines(List<ICoberturaLine> Lines, IDateTimeService dateTimeService)
        {
            this.Lines = Lines;
            _dateTimeService = dateTimeService;
        }
        public List<ICoberturaLine> Lines { get; }

        public bool HasTrackedLines => _trackedLinesState != null;

        public void SetTrackedLines(ITrackedLines trackedLines)
            => _trackedLinesState = new TrackedLinesState(trackedLines, _dateTimeService);

        public void TextViewClosed() => _trackedLinesState?.TextViewClosed();

        public ITrackedLines GetTrackedLinesIfNotOutOfDate(DateTime lastWriteTime)
            => _trackedLinesState.IsOutOfDate(lastWriteTime) ? null : _trackedLinesState.TrackedLines;
    }
}
