using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(IShowFCCOutputPane))]
    internal class ShowFCCOutputPane : IShowFCCOutputPane
    {
        private readonly IFCCOutputWindowPaneCreator _fccOutputWindowCreator;

        [ImportingConstructor]
        public ShowFCCOutputPane(IFCCOutputWindowPaneCreator fccOutputWindowCreator) => this._fccOutputWindowCreator = fccOutputWindowCreator;
        public async Task ShowAsync()
        {
            IFCCOutputWindowPane pane = await this._fccOutputWindowCreator.GetOrCreateAsync();

            if (pane == null)
            {
                return;
            }

            await pane.ShowAsync();
        }
    }
}