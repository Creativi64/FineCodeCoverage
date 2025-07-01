using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletUtil
    {
        void Initialize(string appDataFolder, CancellationToken cancellationToken);

        Task RunCoverletAsync(ICoverageProject project, CancellationToken cancellationToken);
    }
}
