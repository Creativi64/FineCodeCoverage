using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackedLinesFactory
    {
        ITrackedLines Create(List<ICoberturaLine> coberturaLines, ITextSnapshot textSnapshot, string filePath);
    }
}