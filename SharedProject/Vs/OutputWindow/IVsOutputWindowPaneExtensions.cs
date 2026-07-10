using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Vs.OutputWindow
{
    internal static class IVsOutputWindowPaneExtensions
    {
        public static void OutputStringNoPump(this IVsOutputWindowPane pane, string pszOutputString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (pane is IVsOutputWindowPaneNoPump noPumpPane)
            {
                noPumpPane.OutputStringNoPump(pszOutputString);
            }
            else
            {
                Verify.HResult(pane.OutputStringThreadSafe(pszOutputString));
            }
        }
    }
}
