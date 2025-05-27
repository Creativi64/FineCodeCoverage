using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IShellPackageLoader))]
    internal class ShellPackageLoader : IShellPackageLoader
    {
        private readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public ShellPackageLoader(
            [Import(typeof(SVsServiceProvider))]
             IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task LoadPackageAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (serviceProvider.GetService(typeof(SVsShell)) is IVsShell shell)
            {
                var packageToBeLoadedGuid = PackageGuids.guidFCCPackage;
                shell.LoadPackage(ref packageToBeLoadedGuid, out var _);
            }
        }
    }
}
