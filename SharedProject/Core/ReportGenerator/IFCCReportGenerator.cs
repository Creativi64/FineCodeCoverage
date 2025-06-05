using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal interface IFCCReportGenerator
    {
        void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger);

        IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes);
    }
}
