using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal interface IReportResult
    {
        IReadOnlyList<IAssembly> Assemblies { get; }
        IReadOnlyList<MetricType> MetricTypes { get; }
    }
}