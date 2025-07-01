using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IReportResult
    {
        IReadOnlyList<IAssembly> Assemblies { get; }

        IReadOnlyList<MetricType> MetricTypes { get; }
    }
}
