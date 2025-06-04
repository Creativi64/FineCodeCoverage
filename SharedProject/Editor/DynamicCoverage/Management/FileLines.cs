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
                this.TrackedLines = trackedLines;
                this._dateTimeService = dateTimeService;
                this._lastTracked = this._dateTimeService.Now;
            }
            public ITrackedLines TrackedLines { get; }

            internal bool IsOutOfDate(DateTime lastWriteTime) => lastWriteTime > this._lastTracked;
            internal void TextViewClosed() => this._lastTracked = this._dateTimeService.Now;
        }

        private TrackedLinesState _trackedLinesState;
        private readonly IDateTimeService _dateTimeService;

        public FileLines(List<ICoberturaLine> Lines, IDateTimeService dateTimeService)
        {
            this.Lines = Lines;
            this._dateTimeService = dateTimeService;
        }
        public List<ICoberturaLine> Lines { get; }

        public bool HasTrackedLines => this._trackedLinesState != null;

        public void SetTrackedLines(ITrackedLines trackedLines)
            => this._trackedLinesState = new TrackedLinesState(trackedLines, this._dateTimeService);

        public void TextViewClosed() => this._trackedLinesState?.TextViewClosed();

        public ITrackedLines GetTrackedLinesIfNotOutOfDate(DateTime lastWriteTime)
            => this._trackedLinesState.IsOutOfDate(lastWriteTime) ? null : this._trackedLinesState.TrackedLines;
    }
}