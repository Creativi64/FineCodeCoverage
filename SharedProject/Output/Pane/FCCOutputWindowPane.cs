using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Output.Pane
{
    internal class FCCOutputWindowPane : IFCCOutputWindowPane
    {
        private readonly IVsOutputWindowPane _outputWindowPane;
        private readonly Window _outputWindowWindow;
        private readonly TextDocument _fccPaneTextDocument;

        public FCCOutputWindowPane(
            IVsOutputWindowPane outputWindowPane,
            Window outputWindowWindow,
            TextDocument fccPaneTextDocument
        )
        {
            this._outputWindowPane = outputWindowPane;
            this._outputWindowWindow = outputWindowWindow;
            this._fccPaneTextDocument = fccPaneTextDocument;
        }

        public async System.Threading.Tasks.Task OutputStringThreadSafeAsync(string text)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            this._outputWindowPane.OutputStringNoPump(text);
        }

        public async System.Threading.Tasks.Task ShowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            this._outputWindowWindow.Activate();
            _ = this._outputWindowPane.Activate();
        }

        public async System.Threading.Tasks.Task<string> GetTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EditPoint editPoint = this._fccPaneTextDocument.StartPoint.CreateEditPoint();
            return editPoint.GetText(this._fccPaneTextDocument.EndPoint);
        }
    }
}
