using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    internal sealed class NewReportMessage
    {
        public NewReportMessage(IReportResult report, List<ICoverageProject> coverageProjects)
        {
            Report = report;
            CoverageProjects = coverageProjects;
        }

        public IReportResult Report { get; set; }

        public List<ICoverageProject> CoverageProjects { get; }
    }
}
