using System.ComponentModel.Composition;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IPackageLoader))]
    [Export(typeof(IInitializedFromTestContainerDiscoverer))]
    internal class PackageLoader : IPackageLoader, IInitializedFromTestContainerDiscoverer
    {
        private readonly IShellPackageLoader _shellPackageLoader;

        public bool InitializedFromTestContainerDiscoverer { get; private set; }

        [ImportingConstructor]
        public PackageLoader(
            IShellPackageLoader shellPackageLoader
            ) => this._shellPackageLoader = shellPackageLoader;

        public async Task LoadPackageAsync(CancellationToken cancellationToken)
        {
            this.InitializedFromTestContainerDiscoverer = true;
            cancellationToken.ThrowIfCancellationRequested();
            await this._shellPackageLoader.LoadPackageAsync();

        }
    }
}