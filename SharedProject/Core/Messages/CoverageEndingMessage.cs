using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Messages
{
    internal class CoverageEndedMessage
    {
        public CoverageEndedMessage(List<ICoverageProject> coverageProjects)
        {
            this.CoverageProjects = coverageProjects;
        }

        public List<ICoverageProject> CoverageProjects { get; }
    }
}
