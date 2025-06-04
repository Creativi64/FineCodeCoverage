using System;
using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(ISourceFileOpener))]
    internal class SourceFileOpener : ISourceFileOpener
    {
        private readonly AsyncLazy<DTE2> _lazyDTE2;

        [ImportingConstructor]
        public SourceFileOpener(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        ) => this._lazyDTE2 = new AsyncLazy<DTE2>(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return (DTE2)serviceProvider.GetService(typeof(DTE));
        }, ThreadHelper.JoinableTaskFactory);

        public async System.Threading.Tasks.Task OpenAsync(string sourceFilePath, int line)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte = await this._lazyDTE2.GetValueAsync();
            dte.MainWindow.Activate();

            _ = dte.ItemOperations.OpenFile(sourceFilePath, Constants.vsViewKindCode);
            if (line != 0)
            {
                ((TextSelection)dte.ActiveDocument.Selection).GotoLine(line, false);
            }
        }
    }
}