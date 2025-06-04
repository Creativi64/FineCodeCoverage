using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class PalmmediaReportResult : IReportResult
    {
        public PalmmediaReportResult(ParserResult parserResult)
        {
            s_staticMetricTypes.Clear();
            if (parserResult.SupportsBranchCoverage)
            {
                _ = s_staticMetricTypes.Add(MetricType.Branches);
            }

            this.Assemblies = parserResult.Assemblies.Select(a => (IAssembly)new PalmmediaAssembly(a)).ToList();
        }

        public IReadOnlyList<MetricType> MetricTypes => s_staticMetricTypes.ToList();

        private static readonly HashSet<MetricType> s_staticMetricTypes = new HashSet<MetricType>();

        public static void AddMetricTypes(List<MetricType> metricTypes)
            => metricTypes.ForEach(metricType => s_staticMetricTypes.Add(metricType));

        public IReadOnlyList<IAssembly> Assemblies { get; }
    }
}
