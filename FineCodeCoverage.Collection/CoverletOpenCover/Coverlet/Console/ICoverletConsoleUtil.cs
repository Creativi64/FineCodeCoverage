using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    internal interface ICoverletConsoleUtil
    {
        void Initialize(string appDataFolder, CancellationToken cancellationToken);

        Task RunAsync(ICoverageProject project, CancellationToken cancellationToken);
    }
}
