using System;
using System.ComponentModel.Composition;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(ISolutionEvents))]
    public class SolutionEvents : ISolutionEvents, IVsSolutionEvents
    {
        public event EventHandler AfterClosing;

        public event EventHandler AfterOpen;

        [ImportingConstructor]
        public SolutionEvents(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
            )
#pragma warning disable VSTHRD104 // Offer async methods
            => ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var vsSolution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
                Assumes.Present(vsSolution);
                _ = vsSolution.AdviseSolutionEvents(this, out _);
            });
#pragma warning restore VSTHRD104 // Offer async methods

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => VSConstants.S_OK;

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.S_OK;

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => VSConstants.S_OK;

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.S_OK;

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.S_OK;

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.S_OK;

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            AfterOpen?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.S_OK;

        public int OnBeforeCloseSolution(object pUnkReserved) => VSConstants.S_OK;

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            AfterClosing?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }
    }
}
