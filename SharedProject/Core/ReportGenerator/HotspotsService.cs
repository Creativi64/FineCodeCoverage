using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.ReportGeneration
{
    [Export(typeof(IHotspotsService))]
    [ExcludeFromCodeCoverage]
    internal class HotspotsService : IHotspotsService
    {
        internal interface IMethodMetric
        {
            string FullName { get; }
            int Line { get; }
            IEnumerable<IMetric> Metrics { get; }
        }

        internal class RiskHotspot
        {
            public RiskHotspot(string assembly, string @class, IMethodMetric methodMetric)
            {
                this.Assembly = assembly;
                this.Class = @class;
                this.MethodMetric = methodMetric;
            }
            public string Assembly { get; }
            public string Class { get; }
            public IMethodMetric MethodMetric { get; }
        }

        internal class MethodMetric : IMethodMetric
        {
            class Metric : IMetric
            {
                public Metric(string name, decimal value)
                {
                    this.Name = name;
                    this.Value = value;
                }
                public string Name { get; }
                public decimal Value { get; }
            }

            public MethodMetric(ICodeElement codeElement, HotspotThresholdsOptions riskHotspotsAnalysisThresholds)
            {
                this.FullName = codeElement.Name;
                this.Line = codeElement.StartLine;
                var metrics = new List<Metric>();
                if (codeElement.CyclomaticComplexity > riskHotspotsAnalysisThresholds.ThresholdForCyclomaticComplexity)
                {
                    metrics.Add(new Metric("CyclomaticComplexity", codeElement.CyclomaticComplexity));
                }

                if (codeElement.CrapScore > riskHotspotsAnalysisThresholds.ThresholdForCrapScore)
                {
                    metrics.Add(new Metric("CrapScore", codeElement.CrapScore));
                }

                if (codeElement.NPathComplexity > riskHotspotsAnalysisThresholds.ThresholdForNPathComplexity)
                {
                    metrics.Add(new Metric("NPathComplexity", codeElement.NPathComplexity));
                }

                this.IsHotspot = metrics.Any();
                this.Metrics = metrics;

            }
            public bool IsHotspot { get; }
            public string FullName { get; }
            public int Line { get; }
            public IEnumerable<IMetric> Metrics { get; }
        }
        internal interface IMetric
        {
            string Name { get; }
            decimal Value { get; }
        }

        private readonly IOptionsProvider<HotspotThresholdsOptions> hotspotThresholdsOptionsProvider;

        [ImportingConstructor]
        public HotspotsService(
            IOptionsProvider<HotspotThresholdsOptions> hotspotThresholdsOptionsProvider
        ) => this.hotspotThresholdsOptionsProvider = hotspotThresholdsOptionsProvider;

        private static void WriteHotspotsToXml(IEnumerable<RiskHotspot> hotspots, string path)
        {
            var rootElement = new XElement("Hotspots", hotspots.Select(hotspot => new XElement("Hotspot",
                    new XElement("Assembly", hotspot.Assembly),
                    new XElement("Class", hotspot.Class),
                    new XElement("Method", hotspot.MethodMetric.FullName),
                    new XElement("Line", hotspot.MethodMetric.Line),
                    new XElement("Metrics",
                        hotspot.MethodMetric.Metrics.Select(metric => new XElement("Metric",
                                new XElement("Name", metric.Name),
                                new XElement("Value", metric.Value)
                            ))
                    )
                )));

            rootElement.Save(path);
        }

        public void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath)
        {
            IEnumerable<RiskHotspot> riskHotspots = GetRiskhotspots(reportAssemblies, this.hotspotThresholdsOptionsProvider.Get());
            WriteHotspotsToXml(riskHotspots, hotspotsPath);
        }

        private static IEnumerable<RiskHotspot> GetRiskhotspots(IEnumerable<IAssembly> reportAssemblies, HotspotThresholdsOptions hotspotThresholdsOptions)
            => reportAssemblies.SelectMany(assembly =>
                assembly.Classes.SelectMany(cls =>
                    cls.CodeElements.Select(ce => new MethodMetric(ce, hotspotThresholdsOptions))
                        .Where(mm => mm.IsHotspot)
                        .Select(mm => new RiskHotspot(assembly.ShortName, cls.DisplayName, mm)
                    )));
    }
}