using System;
using System.ComponentModel.Composition;
using System.Threading;
using FineCodeCoverage.VSAbstractions.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IDisposeAwareTaskRunner))]
    internal sealed class DisposeAwareTaskRunner : IDisposable, IDisposeAwareTaskRunner
    {
        private readonly CancellationTokenSource _disposeCancellationTokenSource = new CancellationTokenSource();

        internal DisposeAwareTaskRunner()
        {
            JoinableTaskCollection = ThreadHelper.JoinableTaskContext.CreateCollection();
            JoinableTaskFactory = ThreadHelper.JoinableTaskContext.CreateFactory(JoinableTaskCollection);
        }

        private JoinableTaskFactory JoinableTaskFactory { get; }

        private JoinableTaskCollection JoinableTaskCollection { get; }

        /// <summary>
        /// Gets a <see cref="CancellationToken"/> that can be used to check if the package has been disposed.
        /// </summary>
        private CancellationToken DisposalToken => _disposeCancellationTokenSource.Token;

        public bool IsVsShutdown => DisposalToken.IsCancellationRequested;

        public ICancellationTokenSource CreateLinkedTokenSource()
            => new CancellationTokenSourceWrapper(
                CancellationTokenSource.CreateLinkedTokenSource(DisposalToken));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _disposeCancellationTokenSource.Cancel();

            try
            {
                // Block Dispose until all async work has completed.
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
                ThreadHelper.JoinableTaskFactory.Run(JoinableTaskCollection.JoinTillEmptyAsync);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
            }
            catch (OperationCanceledException)
            {
                // this exception is expected because we signaled the cancellation token
            }
            catch (AggregateException ex)
            {
                // ignore AggregateException containing only OperationCanceledException
                ex.Handle(inner => inner is OperationCanceledException);
            }
            finally
            {
                _disposeCancellationTokenSource.Dispose();
            }
        }

        public void RunAsyncFunc(Func<Task> taskProvider)
            => _ = JoinableTaskFactory.RunAsync(taskProvider);
    }
}
