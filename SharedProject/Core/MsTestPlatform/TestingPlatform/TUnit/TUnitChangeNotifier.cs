using System;
using System.ComponentModel.Composition;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitChangeNotifier))]
    internal class TUnitChangeNotifier : ITUnitChangeNotifier, IVsSolutionEvents
    {
        public event EventHandler<ProjectAddedRemoved> ProjectAddedRemovedEvent;
        public event EventHandler SolutionClosedEvent;
        public event EventHandler SolutionOpenedEvent;

        [ImportingConstructor]
        public TUnitChangeNotifier(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
        => ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            _ = vsSolution.AdviseSolutionEvents(this, out _);
        });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously

        #region solution events
        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (fAdded == 1)
            {
                ProjectAddedRemovedEvent?.Invoke(this, new ProjectAddedRemoved(true, pHierarchy));
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            if (fRemoved == 1)
            {
                ProjectAddedRemovedEvent?.Invoke(this, new ProjectAddedRemoved(false, pHierarchy));
            }

            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.S_OK;

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.S_OK;

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.S_OK;

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.S_OK;

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            SolutionOpenedEvent?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.S_OK;

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            SolutionClosedEvent?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved) => VSConstants.S_OK;
        #endregion
    }
}