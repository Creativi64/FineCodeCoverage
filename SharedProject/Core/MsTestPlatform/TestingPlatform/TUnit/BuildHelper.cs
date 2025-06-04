using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(IBuildHelper))]
    internal class BuildHelper : IBuildHelper
    {
        private IVsSolutionBuildManager2 _solutionBuildManager2;
        private IVsSolutionBuildManager3 _solutionBuildManager3;
        private BuildStartEnd _buildStartEnd;
        private bool _building;
        public event EventHandler<BuildStartEndArgs> ExternalBuildEvent;

        [ImportingConstructor]
        public BuildHelper(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            => ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _solutionBuildManager2 = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
                Assumes.Present(_solutionBuildManager2);
                _solutionBuildManager3 = _solutionBuildManager2 as IVsSolutionBuildManager3;
                _buildStartEnd = new BuildStartEnd();
                _ = _solutionBuildManager2.AdviseUpdateSolutionEvents(_buildStartEnd, out uint cookie);
                _buildStartEnd.BuildEvent += BuildStartEnd_BuildEvent;
            });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously

        private void BuildStartEnd_BuildEvent(object sender, BuildStartEndArgs e)
        {
            if (_building)
            {
                return;
            }

            ExternalBuildEvent?.Invoke(this, e);
        }

        public async Task<bool> BuildAsync(List<IVsHierarchy> projects, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            return !await RequiresBuildAsync(cancellationToken) || await BuildAsync(cancellationToken);
        }

        private async Task<bool> BuildAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var buildHandler = new BuildCompletionHandler(cancellationToken);
            int hr = _solutionBuildManager2.AdviseUpdateSolutionEvents(buildHandler, out uint cookie);
            _ = ErrorHandler.ThrowOnFailure(hr);
            bool succeeded = false;
            try
            {
                _building = true;
                int result = _solutionBuildManager2.StartSimpleUpdateSolutionConfiguration(
                    (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD, 0, 1);
                _ = ErrorHandler.ThrowOnFailure(result);
                succeeded = await buildHandler.BuildCompleted;
            }
            catch (OperationCanceledException)
            {
                _ = _solutionBuildManager2.CancelUpdateSolutionConfiguration();
                throw;
            }
            finally
            {
                _building = false;
                buildHandler.Dispose();
                _ = _solutionBuildManager2.UnadviseUpdateSolutionEvents(cookie);
            }

            return succeeded;
        }

        private async Task<bool> RequiresBuildAsync(CancellationToken cancellationToken)
        {
            const bool respectOnlyBuildStartupProjectsAndDependenciesOnRun = false;
            if (!respectOnlyBuildStartupProjectsAndDependenciesOnRun) return true;
#pragma warning disable CS0162 // Unreachable code detected
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
#pragma warning restore CS0162 // Unreachable code detected
            if (_solutionBuildManager3 == null)
                return true;

            int hr = _solutionBuildManager3.AreProjectsUpToDate((uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD);
            return hr != VSConstants.S_OK;
        }
    }
}
