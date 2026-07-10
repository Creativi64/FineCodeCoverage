using System.Collections.Generic;
using FineCodeCoverage.Collection.ReportGeneration;

namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    internal interface IReportFileLineCoverageFactory
    {
        IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies);
    }
}
