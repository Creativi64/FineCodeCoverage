using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IHotspotsService
    {
        RiskHotspotsAnalysisThresholds GetRiskHotspotsAnalysisThresholds();
        void WriteHotspotsToXml(IReadOnlyCollection<RiskHotspot> hotspots, string path);
    }
}
