using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class BuildStartEnd : IVsUpdateSolutionEvents
    {
        public event EventHandler<BuildStartEndArgs> BuildEvent;

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            BuildEvent?.Invoke(this, new BuildStartEndArgs(false));
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            BuildEvent?.Invoke(this, new BuildStartEndArgs(true));
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }
    }

}
