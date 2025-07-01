using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleExecuteRequestProvider
    {
        Task<ExecuteRequest> GetExecuteRequestAsync(ICoverageProject project, string coverletSettings);
    }
}
