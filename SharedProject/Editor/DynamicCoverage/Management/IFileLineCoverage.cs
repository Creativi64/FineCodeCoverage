using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IFileLines
    {
        List<ICoberturaLine> Lines { get; }
        bool HasTrackedLines { get; }
        void SetTrackedLines(ITrackedLines trackedLines);
        void TextViewClosed();
        ITrackedLines GetTrackedLinesIfNotOutOfDate(DateTime lastWriteTime);
    }

    internal interface IFileLineCoverage
    {
        IFileLines GetLines(string filePath);
        void OutOfDate(string filePath);
    }
}
