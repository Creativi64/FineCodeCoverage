using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.TestExplorer
{
    internal interface ITestOperation
    {
        long FailedTests { get; }

        long TotalTests { get; }

        System.Threading.Tasks.Task<List<ICoverageProject>> GetCoverageProjectsAsync();

        string SolutionDirectory { get; }
    }
}
