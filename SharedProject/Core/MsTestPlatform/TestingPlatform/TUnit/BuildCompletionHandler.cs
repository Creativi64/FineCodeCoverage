using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Threading.Tasks;
using System.Threading;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class BuildCompletionHandler : IVsUpdateSolutionEvents, IDisposable
    {
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        private readonly CancellationTokenRegistration registration;
        public BuildCompletionHandler(CancellationToken cancellationToken)
        {
            registration = cancellationToken.Register(() => _tcs.TrySetCanceled());
        }

        /// <summary>
        /// Task that completes when the build finishes.
        /// </summary>
        public Task<bool> BuildCompleted => _tcs.Task;

        public int UpdateSolution_Begin(ref int pfCancelUpdate) => VSConstants.S_OK;

        public int UpdateSolution_Cancel() => VSConstants.S_OK;

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            var cancelled = fCancelCommand == 1;
            var nonFailed = fSucceeded == 1;
            var anySucceeded = fModified == 1;

            // Signal the task completion.
            _tcs.TrySetResult(fSucceeded != 0);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

        public void Dispose()
        {
            registration.Dispose();
        }
    }

}
