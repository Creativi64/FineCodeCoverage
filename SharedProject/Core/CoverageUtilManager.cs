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
        private readonly IOpenCoverUtil _openCoverUtil;
        private readonly ICoverletUtil _coverletUtil;

        [ImportingConstructor]
        public CoverageUtilManager(IOpenCoverUtil openCoverUtil, ICoverletUtil coverletUtil)
        {
            this._openCoverUtil = openCoverUtil;
            this._coverletUtil = coverletUtil;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            this._openCoverUtil.Initialize(appDataFolderPath, cancellationToken);
            this._coverletUtil.Initialize(appDataFolderPath, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task RunCoverageAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            if (project.IsDotNetSdkStyle())
            {
                await this._coverletUtil.RunCoverletAsync(project, cancellationToken);
            }
            else
            {
                await this._openCoverUtil.RunOpenCoverAsync(project, cancellationToken);
            }
        }
    }
}
