namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    public interface IThreadHelper
    {
        IJoinableTaskFactory JoinableTaskFactory { get; }
    }
}
