using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IFirstTimeToolWindowOpener))]
    internal sealed class FirstTimeToolWindowOpener : IFirstTimeToolWindowOpener
    {
        private readonly IInitializedFromTestContainerDiscoverer _initializedFromTestContainerDiscoverer;
        private readonly IShownReportToolWindowHistory _shownToolWindowHistory;
        private readonly IReportToolWindowOpener _toolWindowOpener;

        [ImportingConstructor]
        public FirstTimeToolWindowOpener(
            IInitializedFromTestContainerDiscoverer initializedFromTestContainerDiscoverer,
            IShownReportToolWindowHistory shownToolWindowHistory,
            IReportToolWindowOpener toolWindowOpener)
        {
            _initializedFromTestContainerDiscoverer = initializedFromTestContainerDiscoverer;
            _shownToolWindowHistory = shownToolWindowHistory;
            _toolWindowOpener = toolWindowOpener;
        }

        public async Task OpenIfFirstTimeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (
                !_initializedFromTestContainerDiscoverer.InitializedFromTestContainerDiscoverer ||
                _shownToolWindowHistory.HasShownToolWindow)
            {
                return;
            }

            await _toolWindowOpener.TryOpenAsync();
        }
    }
}
