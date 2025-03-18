using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(AsyncServiceProviderProvider))]
    internal class AsyncServiceProviderProvider
    {
        public IAsyncServiceProvider Provider { get; set; }
    }
}
