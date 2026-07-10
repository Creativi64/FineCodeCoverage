using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Vs.Settings.Services
{
    [Export(typeof(IClearSettingsOnShutdown))]
    internal sealed class ClearSettingsOnShutdown : IClearSettingsOnShutdown
    {
        public const string ClearSettingsOnShutdownOption = "FCCClearSettingsOnShutdown";

        [ImportingConstructor]
        public ClearSettingsOnShutdown(
            [Import(typeof(SVsServiceProvider))]
            System.IServiceProvider serviceProvider)
            => LazyShouldClearSettingsOnShutdown = new AsyncLazy<bool>(
                async () =>
                {
                    if (!Debugger.IsAttached)
                    {
                        return false;
                    }

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    var cmdLine = (IVsAppCommandLine)serviceProvider.GetService(typeof(SVsAppCommandLine));
                    Assumes.Present(cmdLine);
                    int hr = cmdLine.GetOption(ClearSettingsOnShutdownOption, out int isPresent, out _);
                    _ = ErrorHandler.ThrowOnFailure(hr);
                    return isPresent != 0;
                },
                ThreadHelper.JoinableTaskFactory);

        public AsyncLazy<bool> LazyShouldClearSettingsOnShutdown { get; }
    }
}
