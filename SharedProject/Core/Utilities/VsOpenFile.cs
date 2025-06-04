using System.ComponentModel.Composition;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsOpenFile))]
    internal class VsOpenFile : IVsOpenFile
    {
        private readonly DTE2 _dte;

        public VsOpenFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            Assumes.Present(_dte);
        }
        public void OpenFileInCodeEditor(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = _dte.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindCode);
        }

        public void OpenFileInDefaultViewer(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = _dte.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindPrimary);
        }
    }
}
