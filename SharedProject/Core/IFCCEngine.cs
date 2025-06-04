using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    internal interface IFCCEngine
    {
        void StopCoverage();
        void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback);
        void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null);
    }
}
