using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{

    /*
        Idea was to use Nuget api, but
        IVsPackageInstallerEvents
        These events are only raised for packages.config projects. 
        To get updates for both packages.config and PackageReference use IVsNuGetProjectUpdateEvents instead.

        But IVsNuGetProjectUpdateEvents shipped in version 6.2 - Visual Studio 2022

        VSProject interfaces only alternative.
    */

    [Export(typeof(ITUnitChangeNotifier))]
    internal class TUnitChangeNotifier : ITUnitChangeNotifier, IVsSolutionEvents
    {
        public event EventHandler PackageChangeEvent;
        public event EventHandler<ProjectAddedRemoved> ProjectAddedRemovedEvent;
        public event EventHandler SolutionClosedEvent;

        [ImportingConstructor]
        public TUnitChangeNotifier(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                vsSolution.AdviseSolutionEvents(this, out uint _);
            });
        }


        #region solution events
        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (fAdded == 1)
            {
                ProjectAddedRemovedEvent?.Invoke(this, new ProjectAddedRemoved(true, pHierarchy));
            }
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            if(fRemoved == 1)
            {
                ProjectAddedRemovedEvent?.Invoke(this, new ProjectAddedRemoved(false, pHierarchy));
            }
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            SolutionClosedEvent?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }
        #endregion


    }

}
