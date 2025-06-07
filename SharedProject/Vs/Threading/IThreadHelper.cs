namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    internal interface IThreadHelper
    {
        IJoinableTaskFactory JoinableTaskFactory { get; }
    }
}
