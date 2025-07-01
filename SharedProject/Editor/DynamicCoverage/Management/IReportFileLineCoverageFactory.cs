using System.Collections.Generic;
using FineCodeCoverage.Collection.ReportGeneration;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IReportFileLineCoverageFactory
    {
        IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies);
    }
}
