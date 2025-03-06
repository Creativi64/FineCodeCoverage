using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace FineCodeCoverage.Core.Initialization
{
    interface IClearSettingsOnShutdown
    {
        AsyncLazy<bool> LazyShouldClearSettingsOnShutdown { get; }
    }

    [Export(typeof(IClearSettingsOnShutdown))]
    internal class ClearSettingsOnShutdown : IClearSettingsOnShutdown
    {
        public const string ClearSettingsOnShutdownOption = "FCCClearSettingsOnShutdown";


        [ImportingConstructor]
        public ClearSettingsOnShutdown(
            [Import(typeof(SVsServiceProvider))]
            System.IServiceProvider serviceProvider
            )
        {
            LazyShouldClearSettingsOnShutdown = new AsyncLazy<bool>(async () =>
            {
                if (Debugger.IsAttached)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    IVsAppCommandLine cmdLine = (IVsAppCommandLine)serviceProvider.GetService(typeof(SVsAppCommandLine));
                    Assumes.Present(cmdLine);
                    cmdLine.GetOption(ClearSettingsOnShutdownOption, out var isPresent, out var _);

                }
                return false;
            }, ThreadHelper.JoinableTaskFactory);
        }

        public AsyncLazy<bool> LazyShouldClearSettingsOnShutdown { get; }

    }
}
