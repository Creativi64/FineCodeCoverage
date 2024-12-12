using System;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IReportGenerator
    {
        bool GenerateReport(IReportConfiguration reportConfiguration, Settings settings, RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds);

        void SetLogger(Action<VerbosityLevel, string> logger);
    }
}
