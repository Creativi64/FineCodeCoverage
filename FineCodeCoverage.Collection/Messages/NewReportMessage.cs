using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Collection.Engine;

namespace FineCodeCoverage.Collection.Messages
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

        public CoverageCollectionResult Collection { get; set; }
    }
}
