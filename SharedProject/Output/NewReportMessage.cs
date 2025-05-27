using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.ReportGenerator;
namespace FineCodeCoverage.Output
{
    internal class NewReportMessage
    {
        public NewReportMessage(IReportResult report, List<ICoverageProject> coverageProjects)
        {
            this.Report = report;
            CoverageProjects = coverageProjects;
        }
        public IReportResult Report { get; set; }
        public List<ICoverageProject> CoverageProjects { get; }
    }

}
