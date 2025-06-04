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
            _coverletDataCollectorUtil = coverletDataCollectorUtil;
            _coverletGlobalUtil = coverletGlobalUtil;
        }
        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            _coverletGlobalUtil.Initialize(appDataFolder, cancellationToken);
            _coverletDataCollectorUtil.Initialize(appDataFolder, cancellationToken);
        }

        public async Task RunCoverletAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            if (await _coverletDataCollectorUtil.CanUseDataCollectorAsync(project))
            {
                await _coverletDataCollectorUtil.RunAsync(cancellationToken);
            }

            await _coverletGlobalUtil.RunAsync(project, cancellationToken);
        }
    }
}
