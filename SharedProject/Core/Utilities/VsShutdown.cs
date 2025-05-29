using System;
using System.ComponentModel.Composition;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.Utilities
{

    [Export(typeof(IVsShutdown))]
    public class VsShutdown : IVsShutdown, IVsShellPropertyEvents
    {
        public event EventHandler<EventArgs> Shutdown;
        [ImportingConstructor]
        public VsShutdown(
             [Import(typeof(SVsServiceProvider))]
             IServiceProvider serviceProvider

        )
        {
#pragma warning disable VSTHRD104 // Offer async methods
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var vsShell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
                Assumes.Present(vsShell);
                _ = vsShell.AdviseShellPropertyChanges(this, out uint cookie);
            });
#pragma warning restore VSTHRD104 // Offer async methods

        }

        public int OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID6.VSSPROPID_ShutdownStarted)
            {
                Shutdown?.Invoke(this, EventArgs.Empty);
            }

            return VSConstants.S_OK;
        }
    }

}
