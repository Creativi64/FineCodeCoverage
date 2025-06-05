using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IShellPackageLoader))]
    internal class ShellPackageLoader : IShellPackageLoader
    {
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public ShellPackageLoader(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        ) => _serviceProvider = serviceProvider;

        public async Task LoadPackageAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!(_serviceProvider.GetService(typeof(SVsShell)) is IVsShell shell))
            {
                return;
            }

            Guid packageToBeLoadedGuid = PackageGuids.guidFCCPackage;
            _ = shell.LoadPackage(ref packageToBeLoadedGuid, out _);
        }
    }
}
