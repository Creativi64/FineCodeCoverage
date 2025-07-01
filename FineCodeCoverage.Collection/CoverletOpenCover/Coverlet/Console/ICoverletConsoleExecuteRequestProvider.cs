using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverletOpenCover.Process;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    internal interface ICoverletConsoleExecuteRequestProvider
    {
        Task<ExecuteRequest> GetExecuteRequestAsync(ICoverageProject project, string coverletSettings);
    }
}
