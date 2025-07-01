using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleUtil
    {
        void Initialize(string appDataFolder, CancellationToken cancellationToken);

        Task RunAsync(ICoverageProject project, CancellationToken cancellationToken);
    }
}
