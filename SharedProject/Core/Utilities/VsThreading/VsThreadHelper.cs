using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    [Export(typeof(IThreadHelper))]
    internal class VsThreadHelper : IThreadHelper
    {
        public IJoinableTaskFactory JoinableTaskFactory { get; } = new VsJoinableTaskFactory();
    }
}