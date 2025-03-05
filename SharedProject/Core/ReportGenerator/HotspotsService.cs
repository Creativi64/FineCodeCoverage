using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Options;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;

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
                Assembly = assembly;
                Class = @class;
                MethodMetric = methodMetric;
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
                    Name = name;
                    Value = value;
                }
                public string Name { get; }
                public decimal Value { get; }
            }

            public MethodMetric(ICodeElement codeElement, RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds)
            {
                FullName = codeElement.Name;
                Line = codeElement.StartLine;
                List<Metric> metrics = new List<Metric>();
                if(codeElement.CyclomaticComplexity > riskHotspotsAnalysisThresholds.MetricThresholdForCyclomaticComplexity)
                {
                    metrics.Add(new Metric("CyclomaticComplexity", codeElement.CyclomaticComplexity));
                }
                if (codeElement.CrapScore > riskHotspotsAnalysisThresholds.MetricThresholdForCrapScore)
                {
                    metrics.Add(new Metric("CrapScore", codeElement.CrapScore));
                }
                if (codeElement.NPathComplexity > riskHotspotsAnalysisThresholds.MetricThresholdForNPathComplexity)
                {
                    metrics.Add(new Metric("NPathComplexity", codeElement.NPathComplexity));
                }
                IsHotspot = metrics.Any();
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

        private readonly IAppOptionsProvider appOptionsProvider;

        [ImportingConstructor]
        public HotspotsService(
            IAppOptionsProvider appOptionsProvider
        ) => this.appOptionsProvider = appOptionsProvider;

        public RiskHotspotsAnalysisThresholds GetRiskHotspotsAnalysisThresholds() => this.GetRiskHotspotsAnalysisThresholds(this.appOptionsProvider.Get());

        private void WriteHotspotsToXml(IEnumerable<RiskHotspot> hotspots, string path)
        {
            var rootElement = new XElement("Hotspots", hotspots.Select(hotspot =>
            {
                return new XElement("Hotspot",
                    new XElement("Assembly", hotspot.Assembly),
                    new XElement("Class", hotspot.Class),
                    new XElement("Method", hotspot.MethodMetric.FullName),
                    new XElement("Line", hotspot.MethodMetric.Line),
                    new XElement("Metrics",
                        hotspot.MethodMetric.Metrics.Select(metric =>
                        {
                            return new XElement("Metric",
                                new XElement("Name", metric.Name),
                                new XElement("Value", metric.Value)
                            );
                        })
                    )
                );
            }));

            rootElement.Save(path);
        }

        public void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath)
        {
            var riskHotspotsAnalysisThresholds = GetRiskHotspotsAnalysisThresholds();
            var riskHotspots = GetRiskhotspots(reportAssemblies, riskHotspotsAnalysisThresholds);
            WriteHotspotsToXml(riskHotspots, hotspotsPath);
        }

        private IEnumerable<RiskHotspot> GetRiskhotspots(IEnumerable<IAssembly> reportAssemblies, RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds)
            => reportAssemblies.SelectMany(assembly =>
                assembly.Classes.SelectMany(cls =>
                    cls.CodeElements.Select(ce => new MethodMetric(ce, riskHotspotsAnalysisThresholds))
                        .Where(mm => mm.IsHotspot)
                        .Select(mm => new RiskHotspot(assembly.ShortName, cls.DisplayName, mm)
                    )));

        private RiskHotspotsAnalysisThresholds GetRiskHotspotsAnalysisThresholds(IAppOptions appOptions) => new RiskHotspotsAnalysisThresholds
        {
            MetricThresholdForCyclomaticComplexity = appOptions.ThresholdForCyclomaticComplexity,
            MetricThresholdForCrapScore = appOptions.ThresholdForCrapScore,
            MetricThresholdForNPathComplexity = appOptions.ThresholdForNPathComplexity
        };
    }
}
