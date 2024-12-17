using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output.Pane
{
    internal class FCCOutputWindowPane : IFCCOutputWindowPane
    {
        private readonly IVsOutputWindowPane outputWindowPane;
        private readonly Window outputWindowWindow;
        private readonly TextDocument fccPaneTextDocument;

        public FCCOutputWindowPane(
            IVsOutputWindowPane outputWindowPane, 
            Window outputWindowWindow, 
            TextDocument fccPaneTextDocument
        )
        {
            this.outputWindowPane = outputWindowPane;
            this.outputWindowWindow = outputWindowWindow;
            this.fccPaneTextDocument = fccPaneTextDocument;
        }

        public async System.Threading.Tasks.Task OutputStringThreadSafeAsync(string text)
            => await ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _ = this.outputWindowPane.OutputStringThreadSafe(text);
                });

        public async System.Threading.Tasks.Task ShowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            this.outputWindowWindow.Activate();
            _ = this.outputWindowPane.Activate();
        }

        public async System.Threading.Tasks.Task<string> GetTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EditPoint editPoint = this.fccPaneTextDocument.StartPoint.CreateEditPoint();
            return editPoint.GetText(this.fccPaneTextDocument.EndPoint);
        }
    }
}
