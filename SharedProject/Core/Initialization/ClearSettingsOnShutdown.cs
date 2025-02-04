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
                IVsAppCommandLine cmdLine = (IVsAppCommandLine)serviceProvider.GetService(typeof(SVsAppCommandLine));
                cmdLine.GetOption(ClearSettingsOnShutdownOption, out var isPresent, out var _);
                if(isPresent != 0)
                {
                    foreach (var clearSettingsOnShutdown in clearSettingsOnShutdowns)
                    {
                        clearSettingsOnShutdown.ClearSettingsOnShutdown = true;
                    }
                }
                
            }
            
        }
        public const string ClearSettingsOnShutdownOption = "FCCClearSettingsOnShutdown";
    }
}
