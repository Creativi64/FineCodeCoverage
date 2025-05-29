using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class PalmmediaReportResult : IReportResult
    {
        public PalmmediaReportResult(ParserResult parserResult)
        {
            staticMetricTypes.Clear();
            if (parserResult.SupportsBranchCoverage)
            {
                _ = staticMetricTypes.Add(MetricType.Branches);
            }

            this.Assemblies = parserResult.Assemblies.Select(a => (IAssembly)new PalmmediaAssembly(a)).ToList();
        }

        public IReadOnlyList<MetricType> MetricTypes => staticMetricTypes.ToList();

        private static readonly HashSet<MetricType> staticMetricTypes = new HashSet<MetricType>();

        public static void AddMetricTypes(List<MetricType> metricTypes)
            => metricTypes.ForEach(metricType => staticMetricTypes.Add(metricType));

        public IReadOnlyList<IAssembly> Assemblies { get; }
    }
}
