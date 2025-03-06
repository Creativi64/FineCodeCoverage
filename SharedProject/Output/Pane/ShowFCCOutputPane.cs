using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(IShowFCCOutputPane))]
    internal class ShowFCCOutputPane : IShowFCCOutputPane
    {
        private readonly IFCCOutputWindowPaneCreator fccOutputWindowCreator;

        [ImportingConstructor]
        public ShowFCCOutputPane(IFCCOutputWindowPaneCreator fccOutputWindowCreator)
        {
            this.fccOutputWindowCreator = fccOutputWindowCreator;
        }
        public async Task ShowAsync()
        {
            var pane = await this.fccOutputWindowCreator.GetOrCreateAsync();

            if (pane != null)
            {
                await pane.ShowAsync();
            }
        }
    }
}
