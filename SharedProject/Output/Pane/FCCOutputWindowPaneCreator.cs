using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Output.Pane
{

    [Export(typeof(IFCCOutputWindowPaneCreator))]
    internal class FCCOutputWindowPaneCreator : IFCCOutputWindowPaneCreator
    {
        private const string fccPaneGuidString = "{3B3C775A-0050-445D-9022-0230957805B2}";
        
        private readonly IServiceProvider _serviceProvider;
        private IFCCOutputWindowPane fccOutputWindowPane;

        [ImportingConstructor]
        public FCCOutputWindowPaneCreator(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        ) => this._serviceProvider = serviceProvider;

        public async System.Threading.Tasks.Task<IFCCOutputWindowPane> GetOrCreateAsync()
        {
            if (this.fccOutputWindowPane != null)
            {
                return this.fccOutputWindowPane;
            }

            await this.SetPaneAsync();
            return this.fccOutputWindowPane;
        }

        private async System.Threading.Tasks.Task SetPaneAsync()
        {
            IVsOutputWindowPane pane = await this.CreatePaneAsync();
            Window outputWindowWindow = await this.GetOutputWindowWindowAsync();
            TextDocument paneTextDocument = await this.GetPaneTextDocumentAsync(outputWindowWindow);

            this.fccOutputWindowPane = new FCCOutputWindowPane(pane, outputWindowWindow, paneTextDocument);
        }

        private async System.Threading.Tasks.Task<IVsOutputWindowPane> CreatePaneAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var fccPaneGuid = Guid.Parse(fccPaneGuidString);
            var outputWindow = (IVsOutputWindow)this._serviceProvider.GetService(typeof(SVsOutputWindow));
            Assumes.Present(outputWindow);

            _ = outputWindow.CreatePane(
                ref fccPaneGuid,
                "FCC",
                Convert.ToInt32(true),
                Convert.ToInt32(false)); // do not clear with solution otherwise will not get initialize methods

            _ = outputWindow.GetPane(ref fccPaneGuid, out IVsOutputWindowPane pane);
            return pane;
        }

        private async System.Threading.Tasks.Task<Window> GetOutputWindowWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = (DTE2)this._serviceProvider.GetService(typeof(EnvDTE.DTE));
            Assumes.Present(dte);
            return dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);

        }

        private async System.Threading.Tasks.Task<TextDocument> GetPaneTextDocumentAsync(Window outputWindowWindow)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return (outputWindowWindow.Object as OutputWindow).OutputWindowPanes.Cast<OutputWindowPane>()
                .First(this.IsFCCPane).TextDocument;
        }

        private bool IsFCCPane(OutputWindowPane owp)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return owp.Guid == fccPaneGuidString;
        }
    }
}
