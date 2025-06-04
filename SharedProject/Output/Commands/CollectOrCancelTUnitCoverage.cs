using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal class CollectOrCancelTUnitCoverage : ICommandInitializer
    {
        private readonly ICollectTUnitCommand _collectTUnitCommand;
        private readonly ICancelCollectTUnitCommand _cancelCollectTUnitCommand;

        [ImportingConstructor]
        public CollectOrCancelTUnitCoverage(
            ICollectTUnitCommand collectTUnitCommand,
            ICancelCollectTUnitCommand cancelCollectTUnitCommand,
            ITUnitCoverage tUnitCoverage)
        {
            tUnitCoverage.CollectingChangedEvent += (_, collecting) =>
            {
                collectTUnitCommand.SetVisible(!collecting);
                cancelCollectTUnitCommand.SetVisible(collecting);
            };
            this._collectTUnitCommand = collectTUnitCommand;
            this._cancelCollectTUnitCommand = cancelCollectTUnitCommand;
        }

        public async Task InitializeAsync(ICommandPackageServices commandPackageServices)
        {
            await this._collectTUnitCommand.InitializeAsync(commandPackageServices);
            await this._cancelCollectTUnitCommand.InitializeAsync(commandPackageServices);
            this._cancelCollectTUnitCommand.SetVisible(false);
        }
    }
}
