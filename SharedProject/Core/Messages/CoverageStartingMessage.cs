using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Messages
{
    internal class CoverageStartingMessage
    {
        public CoverageStartingMessage(bool pending = false) => this.Pending = pending;

        public bool Pending { get; }
    }


    internal class CoverageEndedMessage
    {
        public CoverageEndedMessage(List<ICoverageProject> coverageProjects)
        {
            this.CoverageProjects = coverageProjects;
        }

        public List<ICoverageProject> CoverageProjects { get; }
    }
}
