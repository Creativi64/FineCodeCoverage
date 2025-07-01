using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine
{
    public interface IFCCEngine
    {
        void StopCoverage();

        void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback);

        void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null);
    }
}
