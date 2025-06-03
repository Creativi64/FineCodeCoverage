using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IDisposeAwareTaskRunner))]
    internal class DisposeAwareTaskRunner : IDisposable, IDisposeAwareTaskRunner
    {
        private readonly CancellationTokenSource disposeCancellationTokenSource = new CancellationTokenSource();

        internal DisposeAwareTaskRunner()
        {
            this.JoinableTaskCollection = ThreadHelper.JoinableTaskContext.CreateCollection();
            this.JoinableTaskFactory = ThreadHelper.JoinableTaskContext.CreateFactory(this.JoinableTaskCollection);
        }

        JoinableTaskFactory JoinableTaskFactory { get; }
        JoinableTaskCollection JoinableTaskCollection { get; }

        /// <summary>
        /// Gets a <see cref="CancellationToken"/> that can be used to check if the package has been disposed.
        /// </summary>
        private CancellationToken DisposalToken => this.disposeCancellationTokenSource.Token;

        public bool IsVsShutdown => this.DisposalToken.IsCancellationRequested;

        public ICancellationTokenSource CreateLinkedTokenSource()
            => new CancellationTokenSourceWrapper(
                CancellationTokenSource.CreateLinkedTokenSource(this.DisposalToken));

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.disposeCancellationTokenSource.Cancel();

                try
                {
                    // Block Dispose until all async work has completed.
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
                    ThreadHelper.JoinableTaskFactory.Run(this.JoinableTaskCollection.JoinTillEmptyAsync);
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
                    this.disposeCancellationTokenSource.Dispose();
                }
            }
        }

        public void RunAsyncFunc(Func<Task> taskProvider)
            => _ = this.JoinableTaskFactory.RunAsync(taskProvider);
    }
}