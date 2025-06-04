using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletUtil))]
    internal class CoverletUtil : ICoverletUtil
    {
        private readonly ICoverletDataCollectorUtil _coverletDataCollectorUtil;
        private readonly ICoverletConsoleUtil _coverletGlobalUtil;

        [ImportingConstructor]
        public CoverletUtil(ICoverletDataCollectorUtil coverletDataCollectorUtil, ICoverletConsoleUtil coverletGlobalUtil)
        {
            this._coverletDataCollectorUtil = coverletDataCollectorUtil;
            this._coverletGlobalUtil = coverletGlobalUtil;
        }
        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            this._coverletGlobalUtil.Initialize(appDataFolder, cancellationToken);
            this._coverletDataCollectorUtil.Initialize(appDataFolder, cancellationToken);
        }

        public async Task RunCoverletAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            if (await this._coverletDataCollectorUtil.CanUseDataCollectorAsync(project))
            {
                await this._coverletDataCollectorUtil.RunAsync(cancellationToken);
            }

            await this._coverletGlobalUtil.RunAsync(project, cancellationToken);
        }
    }
}
