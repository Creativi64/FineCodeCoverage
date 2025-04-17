using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft;

namespace FineCodeCoverage.Output.Pane
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
