using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class BuildCompletionHandler : IVsUpdateSolutionEvents, IDisposable
    {
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        private readonly CancellationTokenRegistration _registration;
        public BuildCompletionHandler(CancellationToken cancellationToken)
            => _registration = cancellationToken.Register(() => _tcs.TrySetCanceled());

        /// <summary>
        /// Task that completes when the build finishes.
        /// </summary>
        public Task<bool> BuildCompleted => _tcs.Task;

        public int UpdateSolution_Begin(ref int pfCancelUpdate) => VSConstants.S_OK;

        public int UpdateSolution_Cancel() => VSConstants.S_OK;

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            // bool cancelled = fCancelCommand == 1;
            // bool nonFailed = fSucceeded == 1;
            // bool anySucceeded = fModified == 1;

            // Signal the task completion.
            _ = _tcs.TrySetResult(fSucceeded != 0);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

        public void Dispose() => _registration.Dispose();
    }
}
