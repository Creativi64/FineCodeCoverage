using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal interface IToolWindowServiceInit
    {
        AsyncPackage Package { set; }
    }
}
