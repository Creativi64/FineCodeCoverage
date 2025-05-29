using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Engine.Coverlet;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.OpenCover;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(ICoverageUtilManager))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal class CoverageUtilManager : ICoverageUtilManager, IAppDataFolderPathDependent
    {
        private readonly IOpenCoverUtil openCoverUtil;
        private readonly ICoverletUtil coverletUtil;

        [ImportingConstructor]
        public CoverageUtilManager(IOpenCoverUtil openCoverUtil, ICoverletUtil coverletUtil)
        {
            this.openCoverUtil = openCoverUtil;
            this.coverletUtil = coverletUtil;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            this.openCoverUtil.Initialize(appDataFolderPath, cancellationToken);
            this.coverletUtil.Initialize(appDataFolderPath, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task RunCoverageAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            if (project.IsDotNetSdkStyle())
            {
                await this.coverletUtil.RunCoverletAsync(project, cancellationToken);
            }
            else
            {
                await this.openCoverUtil.RunOpenCoverAsync(project, cancellationToken);
            }
        }
    }
}
