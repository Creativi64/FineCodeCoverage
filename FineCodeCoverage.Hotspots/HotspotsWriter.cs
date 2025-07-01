using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Hotspots
{
    [Export(typeof(IHotspotsWriter))]
    [ExcludeFromCodeCoverage]
    internal sealed class HotspotsWriter : IHotspotsWriter
    {
        internal interface IMethodMetric
        {
            string FullName { get; }

            int Line { get; }

            IEnumerable<IMetric> Metrics { get; }
        }

        internal sealed class RiskHotspot
        {
            public RiskHotspot(string assembly, string @class, IMethodMetric methodMetric)
            {
                Assembly = assembly;
                Class = @class;
                MethodMetric = methodMetric;
            }

            public string Assembly { get; }

            public string Class { get; }

            public IMethodMetric MethodMetric { get; }
        }

        internal sealed class MethodMetric : IMethodMetric
        {
            private sealed class Metric : IMetric
            {
                public Metric(string name, decimal value)
                {
                    Name = name;
                    Value = value;
                }

                public string Name { get; }

                public decimal Value { get; }
            }

            public MethodMetric(ICodeElement codeElement, HotspotThresholdsOptions riskHotspotsAnalysisThresholds)
            {
                FullName = codeElement.Name;
                Line = codeElement.StartLine;
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

                IsHotspot = metrics.Count != 0;
                Metrics = metrics;
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

        private readonly IOptionsProvider<HotspotThresholdsOptions> _hotspotThresholdsOptionsProvider;

        [ImportingConstructor]
        public HotspotsWriter(
            IOptionsProvider<HotspotThresholdsOptions> hotspotThresholdsOptionsProvider) => _hotspotThresholdsOptionsProvider = hotspotThresholdsOptionsProvider;

        private static void WriteHotspotsToXml(IEnumerable<RiskHotspot> hotspots, string path)
        {
            var rootElement = new XElement(
                "Hotspots",
                hotspots.Select(hotspot
                    => new XElement(
                        "Hotspot",
                        new XElement("Assembly", hotspot.Assembly),
                        new XElement("Class", hotspot.Class),
                        new XElement("Method", hotspot.MethodMetric.FullName),
                        new XElement("Line", hotspot.MethodMetric.Line),
                        new XElement(
                            "Metrics",
                            hotspot.MethodMetric.Metrics.Select(metric => new XElement(
                                "Metric",
                                new XElement("Name", metric.Name),
                                new XElement("Value", metric.Value)))))));

            rootElement.Save(path);
        }

        public void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath)
        {
            IEnumerable<RiskHotspot> riskHotspots = GetRiskhotspots(reportAssemblies, _hotspotThresholdsOptionsProvider.Get());
            WriteHotspotsToXml(riskHotspots, hotspotsPath);
        }

        private static IEnumerable<RiskHotspot> GetRiskhotspots(IEnumerable<IAssembly> reportAssemblies, HotspotThresholdsOptions hotspotThresholdsOptions)
            => reportAssemblies.SelectMany(assembly =>
                assembly.Classes.SelectMany(cls =>
                    cls.CodeElements.Select(ce => new MethodMetric(ce, hotspotThresholdsOptions))
                        .Where(mm => mm.IsHotspot)
                        .Select(mm => new RiskHotspot(assembly.ShortName, cls.DisplayName, mm))));
    }
}
