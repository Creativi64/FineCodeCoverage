using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal class CollectOrCancelTUnitCoverage : ICommandInitializer
    {
        private readonly ICollectTUnitCommand collectTUnitCommand;
        private readonly ICancelCollectTUnitCommand cancelCollectTUnitCommand;

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
            this.collectTUnitCommand = collectTUnitCommand;
            this.cancelCollectTUnitCommand = cancelCollectTUnitCommand;
        }

        public async Task InitializeAsync(ICommandPackageServices commandPackageServices)
        {
            await this.collectTUnitCommand.InitializeAsync(commandPackageServices);
            await this.cancelCollectTUnitCommand.InitializeAsync(commandPackageServices);
            this.cancelCollectTUnitCommand.SetVisible(false);
        }
    }
}