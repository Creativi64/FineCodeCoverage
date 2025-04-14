using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedLinesState
    {
        public TrackedLinesState(ITrackedLines trackedLines)
        {
            this.TrackedLines = trackedLines;
            this.Date = DateTime.Now;
        }
        public ITrackedLines TrackedLines { get; }
        public DateTime Date { get; set; }
    }
    internal class FileLines {
        public FileLines(List<ICoberturaLine> Lines) => this.Lines = Lines;
        public List<ICoberturaLine> Lines { get; }
        public TrackedLinesState TrackedLinesState { get; set; }
    }

    internal interface IFileLineCoverage
    {
        FileLines GetLines(string filePath);
    }
}
