using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackedLinesFactory
    {
        ITrackedLines Create(List<ICoberturaLine> coberturaLines, ITextSnapshot textSnapshot, string filePath);
        ITrackedLines Create(string serializedCoverage, ITextSnapshot currentSnapshot, string filePath);
        string Serialize(ITrackedLines trackedLines, string text);
    }
}
