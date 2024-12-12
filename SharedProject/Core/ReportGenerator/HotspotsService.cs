using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using FineCodeCoverage.Options;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;

namespace FineCodeCoverage.ReportGeneration
{
    [Export(typeof(IHotspotsService))]
    [ExcludeFromCodeCoverage]
    internal class HotspotsService : IHotspotsService
    {
        private readonly IAppOptionsProvider appOptionsProvider;

        [ImportingConstructor]
        public HotspotsService(
            IAppOptionsProvider appOptionsProvider
        ) => this.appOptionsProvider = appOptionsProvider;

        public RiskHotspotsAnalysisThresholds GetRiskHotspotsAnalysisThresholds() => this.GetRiskHotspotsAnalysisThresholds(this.appOptionsProvider.Get());

        public void WriteHotspotsToXml(IReadOnlyCollection<RiskHotspot> hotspots, string path)
        {
            var rootElement = new XElement("Hotspots", hotspots.Select(hotspot =>
            {
                return new XElement("Hotspot",
                    new XElement("Assembly", hotspot.Assembly),
                    new XElement("Class", hotspot.Class),
                    new XElement("Method", hotspot.MethodMetric.FullName),
                    new XElement("Line", hotspot.MethodMetric.Line),
                    new XElement("Metrics",
                        hotspot.StatusMetrics.Where(statusMetric => statusMetric.Exceeded).Select(statusMetric =>
                        {
                            return new XElement("Metric",
                                new XElement("Name", statusMetric.Metric.Name),
                                new XElement("Value", statusMetric.Metric.Value)
                            );
                        })
                    )
                );
            }));

            rootElement.Save(path);
        }

        private RiskHotspotsAnalysisThresholds GetRiskHotspotsAnalysisThresholds(IAppOptions appOptions) => new RiskHotspotsAnalysisThresholds
        {
            MetricThresholdForCyclomaticComplexity = appOptions.ThresholdForCyclomaticComplexity,
            MetricThresholdForCrapScore = appOptions.ThresholdForCrapScore,
            MetricThresholdForNPathComplexity = appOptions.ThresholdForNPathComplexity
        };
    }
}
