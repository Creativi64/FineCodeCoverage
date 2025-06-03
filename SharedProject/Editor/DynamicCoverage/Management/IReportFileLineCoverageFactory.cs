using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IReportFileLineCoverageFactory
    {
        IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies);
    }
}