using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.ComponentModel.Composition;

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
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsShell vsShell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
                vsShell.AdviseShellPropertyChanges(this, out var cookie);
            });

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
