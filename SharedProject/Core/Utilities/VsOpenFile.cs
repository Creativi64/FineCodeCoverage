using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsOpenFile))]
    internal class VsOpenFile : IVsOpenFile
    {
        private readonly DTE2 dte;

        public VsOpenFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            Assumes.Present(this.dte);
        }
        public void OpenFileInCodeEditor(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.dte.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindCode);
        }

        public void OpenFileInDefaultViewer(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.dte.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindPrimary);
        }
    }
}
