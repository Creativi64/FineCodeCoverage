using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Utilities.Wrappers
{
    [Export(typeof(IProcess))]
    [ExcludeFromCodeCoverage]
    internal sealed class ProcessWrapper : IProcess
    {
        public void Start(string fileName) => _ = Process.Start(fileName);
    }
}
