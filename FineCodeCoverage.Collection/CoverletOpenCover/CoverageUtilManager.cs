using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverletOpenCover.Coverlet;
using FineCodeCoverage.Collection.CoverletOpenCover.OpenCover;
using FineCodeCoverage.Initialization;

namespace FineCodeCoverage.Collection.CoverletOpenCover
{
    [Export(typeof(ICoverageUtilManager))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal sealed class CoverageUtilManager : ICoverageUtilManager, IAppDataFolderPathDependent
    {
        private readonly IOpenCoverUtil _openCoverUtil;
        private readonly ICoverletUtil _coverletUtil;

        [ImportingConstructor]
        public CoverageUtilManager(IOpenCoverUtil openCoverUtil, ICoverletUtil coverletUtil)
        {
            _openCoverUtil = openCoverUtil;
            _coverletUtil = coverletUtil;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            _openCoverUtil.Initialize(appDataFolderPath, cancellationToken);
            _coverletUtil.Initialize(appDataFolderPath, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task RunCoverageAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            if (project.IsDotNetSdkStyle())
            {
                await _coverletUtil.RunCoverletAsync(project, cancellationToken);
            }
            else
            {
                await _openCoverUtil.RunOpenCoverAsync(project, cancellationToken);
            }
        }
    }
}
