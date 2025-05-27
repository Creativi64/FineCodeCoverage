using System;
using System.Collections.Generic;
using PalmmediaMetric = Palmmedia.ReportGenerator.Core.Parser.Analysis.Metric;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal static class MetricSetter
    {
        private static readonly Dictionary<string, Func<PalmmediaCodeElement, decimal?, MetricType>> metricSetters = new Dictionary<string, Func<PalmmediaCodeElement, decimal?, MetricType>>
        {
            { MetricNames.BlocksCovered, (pce,value) => {
                    pce.BlocksCovered = (int)value;
                    return MetricType.BlocksCovered;
                }
            },
            { MetricNames.BlocksNotCovered, (pce,value) => {
                pce.BlocksNotCovered = (int)value;
                return MetricType.BlocksNotCovered;
            } },
            { MetricNames.Crap, (pce,value) => {
                pce.CrapScore = (int)value;
                return MetricType.Crap;
            } },
            { MetricNames.NPath, (pce,value) => {
                pce.NPathComplexity = (int)value;
                return MetricType.NPath;
            } },
            { MetricNames.CyclomaticComplexity, (pce,value) => {
                pce.CyclomaticComplexity = (int)value;
                return MetricType.CyclomaticComplexity;
            }}
        };

        public static List<MetricType> SetMetricProperties(
            this PalmmediaCodeElement palmmediaCodeElement,
            IEnumerable<PalmmediaMetric> metrics)
        {
            var metricTypes = new List<MetricType>();
            foreach (var metric in metrics)
            {
                metricSetters.TryGetValue(metric.Name, out var setter);
                if (setter != null)
                {

                    var metricType = setter(palmmediaCodeElement, metric.Value);
                    metricTypes.Add(metricType);
                }
            }
            return metricTypes;
        }
    }

}
