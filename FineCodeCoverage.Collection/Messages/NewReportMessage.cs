using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    public sealed class NewReportMessage
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
