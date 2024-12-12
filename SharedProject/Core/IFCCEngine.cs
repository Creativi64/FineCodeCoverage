using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    internal interface IFCCEngine
    {
        string AppDataFolderPath { get; }
        void Initialize(System.Threading.CancellationToken cancellationToken);
        void StopCoverage();
        void ReloadCoverage(Func<System.Threading.Tasks.Task<List<ICoverageProject>>> coverageRequestCallback);
        void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null);
        void ClearUI();
    }

}