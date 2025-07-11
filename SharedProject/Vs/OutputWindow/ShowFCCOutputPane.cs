using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(IShowFCCOutputPane))]
    internal sealed class ShowFCCOutputPane : IShowFCCOutputPane
    {
        private readonly IFCCOutputWindowPaneCreator _fccOutputWindowCreator;

        [ImportingConstructor]
        public ShowFCCOutputPane(IFCCOutputWindowPaneCreator fccOutputWindowCreator) => _fccOutputWindowCreator = fccOutputWindowCreator;

        public async Task ShowAsync()
        {
            IFCCOutputWindowPane pane = await _fccOutputWindowCreator.GetOrCreateAsync();

            if (pane == null)
            {
                return;
            }

            await pane.ShowAsync();
        }
    }
}
