using System.Collections.Generic;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IReportResult
    {
        IReadOnlyList<IAssembly> Assemblies { get; }

        IReadOnlyList<MetricType> MetricTypes { get; }
    }
}
