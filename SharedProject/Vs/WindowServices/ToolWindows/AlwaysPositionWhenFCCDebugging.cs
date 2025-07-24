using System.ComponentModel.Composition;
using FineCodeCoverage.Utilities.Debugging;

namespace FineCodeCoverage.Vs.WindowServices.ToolWindows
{
    [Export(typeof(IShouldAlwaysPositionToolWindows))]
    internal sealed class AlwaysPositionWhenFCCDebugging : IShouldAlwaysPositionToolWindows
    {
        [ImportingConstructor]
        public AlwaysPositionWhenFCCDebugging(IFCCDebugStatus fccDebugStatus) => AlwaysPosition = fccDebugStatus.Debugging;

        public bool AlwaysPosition { get; }
    }
}
