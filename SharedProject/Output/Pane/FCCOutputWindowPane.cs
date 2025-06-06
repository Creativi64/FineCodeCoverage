using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Output.Pane
{
    internal sealed class FCCOutputWindowPane : IFCCOutputWindowPane
    {
        private readonly IVsOutputWindowPane _outputWindowPane;
        private readonly Window _outputWindowWindow;
        private readonly TextDocument _fccPaneTextDocument;

        public FCCOutputWindowPane(
            IVsOutputWindowPane outputWindowPane,
            Window outputWindowWindow,
            TextDocument fccPaneTextDocument)
        {
            _outputWindowPane = outputWindowPane;
            _outputWindowWindow = outputWindowWindow;
            _fccPaneTextDocument = fccPaneTextDocument;
        }

        public async System.Threading.Tasks.Task OutputStringThreadSafeAsync(string text)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _outputWindowPane.OutputStringNoPump(text);
        }

        public async System.Threading.Tasks.Task ShowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _outputWindowWindow.Activate();
            _ = _outputWindowPane.Activate();
        }

        public async System.Threading.Tasks.Task<string> GetTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EditPoint editPoint = _fccPaneTextDocument.StartPoint.CreateEditPoint();
            return editPoint.GetText(_fccPaneTextDocument.EndPoint);
        }
    }
}
