using System.ComponentModel.Composition;
using System.Diagnostics;

namespace FineCodeCoverage.Utilities.Debugging
{
    [Export(typeof(IFCCDebugStatus))]
    internal sealed class FCCDebugStatus : IFCCDebugStatus
    {
        public bool Debugging
        {
            get
            {
                if (!Debugger.IsAttached)
                {
                    return false;
                }
#if DEBUG

                return true;
#else
           return false;
#endif
            }
        }
    }
}
