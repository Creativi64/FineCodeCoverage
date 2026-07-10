using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    internal interface IFCCReportGenerator
    {
        void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger);

        IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes);
    }
}
