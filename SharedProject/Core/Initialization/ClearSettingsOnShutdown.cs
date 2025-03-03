using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace FineCodeCoverage.Core.Initialization
{
    interface IClearSettingsOnShutdown
    {
        bool ClearSettingsOnShutdown { get; set; }
    }

    [Export(typeof(ClearSettingsOnShutdown))]
    internal class ClearSettingsOnShutdown
    {
        [ImportingConstructor]
        public ClearSettingsOnShutdown(
            [ImportMany] IClearSettingsOnShutdown[] clearSettingsOnShutdowns,
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
            )
        {
            if (Debugger.IsAttached)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    IVsAppCommandLine cmdLine = (IVsAppCommandLine)serviceProvider.GetService(typeof(SVsAppCommandLine));
                    Assumes.Present(cmdLine);
                    cmdLine.GetOption(ClearSettingsOnShutdownOption, out var isPresent, out var _);
                    if (isPresent != 0)
                    {
                        foreach (var clearSettingsOnShutdown in clearSettingsOnShutdowns)
                        {
                            clearSettingsOnShutdown.ClearSettingsOnShutdown = true;
                        }
                    }
                }).Join();
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
            }
            
        }
        public const string ClearSettingsOnShutdownOption = "FCCClearSettingsOnShutdown";
    }
}
