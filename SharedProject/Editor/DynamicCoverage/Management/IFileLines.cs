using System;
using System.Collections.Generic;
using FineCodeCoverage.Collection.ReportGeneration;

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
}
