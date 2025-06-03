using System;
using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class FileLines : IFileLines
    {
        internal class TrackedLinesState
        {
            private readonly IDateTimeService dateTimeService;
            private DateTime lastTracked;
            public TrackedLinesState(ITrackedLines trackedLines, IDateTimeService dateTimeService)
            {
                this.TrackedLines = trackedLines;
                this.dateTimeService = dateTimeService;
                this.lastTracked = this.dateTimeService.Now;
            }
            public ITrackedLines TrackedLines { get; }

            internal bool IsOutOfDate(DateTime lastWriteTime) => lastWriteTime > this.lastTracked;
            internal void TextViewClosed() => this.lastTracked = this.dateTimeService.Now;
        }
        public FileLines(List<ICoberturaLine> Lines, IDateTimeService dateTimeService)
        {
            this.Lines = Lines;
            this.dateTimeService = dateTimeService;
        }
        public List<ICoberturaLine> Lines { get; }
        private TrackedLinesState trackedLinesState;
        private readonly IDateTimeService dateTimeService;

        public bool HasTrackedLines => this.trackedLinesState != null;

        public void SetTrackedLines(ITrackedLines trackedLines) => this.trackedLinesState = new TrackedLinesState(trackedLines, this.dateTimeService);
        public void TextViewClosed() => this.trackedLinesState?.TextViewClosed();
        public ITrackedLines GetTrackedLinesIfNotOutOfDate(DateTime lastWriteTime)
            => this.trackedLinesState.IsOutOfDate(lastWriteTime) ? null : this.trackedLinesState.TrackedLines;
    }
}