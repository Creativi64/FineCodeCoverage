using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.Runners;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class CollectOrCancelTUnitCoverage : ICommandInitializer
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
            _collectTUnitCommand = collectTUnitCommand;
            _cancelCollectTUnitCommand = cancelCollectTUnitCommand;
        }

        public async Task InitializeAsync(ICommandPackageServices commandPackageServices)
        {
            await _collectTUnitCommand.InitializeAsync(commandPackageServices);
            await _cancelCollectTUnitCommand.InitializeAsync(commandPackageServices);
            _cancelCollectTUnitCommand.SetVisible(false);
        }
    }
}
