using System;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Output.Pane
{
    [Export(typeof(IFCCOutputWindowPaneCreator))]
    [Export(typeof(IFCCOutputWindowPaneWritableCreator))]
    internal sealed class FCCOutputWindowPaneCreator : IFCCOutputWindowPaneCreator, IFCCOutputWindowPaneWritableCreator
    {
        private const string FCCPaneGuidString = "{3B3C775A-0050-445D-9022-0230957805B2}";
        private readonly IServiceProvider _serviceProvider;
        private FCCOutputWindowPane _fccOutputWindowPane;

        [ImportingConstructor]
        public FCCOutputWindowPaneCreator(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async System.Threading.Tasks.Task<IFCCOutputWindowPane> GetOrCreateAsync()
        {
            FCCOutputWindowPane pane = await GetOrCreateImplAsync();
            return pane;
        }

        public async System.Threading.Tasks.Task<IFCCOutputWindowPaneWritable> GetOrCreateWritableAsync()
        {
            FCCOutputWindowPane pane = await GetOrCreateImplAsync();
            return pane;
        }

        private async System.Threading.Tasks.Task<FCCOutputWindowPane> GetOrCreateImplAsync()
        {
            if (_fccOutputWindowPane != null)
            {
                return _fccOutputWindowPane;
            }

            await SetPaneAsync();
            return _fccOutputWindowPane;
        }

        private async System.Threading.Tasks.Task SetPaneAsync()
        {
            IVsOutputWindowPane pane = await CreatePaneAsync();
            Window outputWindowWindow = await GetOutputWindowWindowAsync();
            TextDocument paneTextDocument = await GetPaneTextDocumentAsync(outputWindowWindow);

            _fccOutputWindowPane = new FCCOutputWindowPane(pane, outputWindowWindow, paneTextDocument);
        }

        private async System.Threading.Tasks.Task<IVsOutputWindowPane> CreatePaneAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var fccPaneGuid = Guid.Parse(FCCPaneGuidString);
            var outputWindow = (IVsOutputWindow)_serviceProvider.GetService(typeof(SVsOutputWindow));
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
            var dte = (DTE2)_serviceProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            return dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
        }

        private async System.Threading.Tasks.Task<TextDocument> GetPaneTextDocumentAsync(Window outputWindowWindow)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ((OutputWindow)outputWindowWindow.Object).OutputWindowPanes.Cast<OutputWindowPane>()
                .First(IsFCCPane).TextDocument;
        }

        private bool IsFCCPane(OutputWindowPane owp)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return owp.Guid == FCCPaneGuidString;
        }
    }
}
