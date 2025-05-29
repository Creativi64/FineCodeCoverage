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
        private IVsSolutionBuildManager2 solutionBuildManager2;
        private IVsSolutionBuildManager3 solutionBuildManager3;
        private BuildStartEnd buildStartEnd;
        private bool building;
        public event EventHandler<BuildStartEndArgs> ExternalBuildEvent;

        [ImportingConstructor]
        public BuildHelper(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                this.solutionBuildManager2 = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
                Assumes.Present(this.solutionBuildManager2);
                this.solutionBuildManager3 = this.solutionBuildManager2 as IVsSolutionBuildManager3;
                buildStartEnd = new BuildStartEnd();
                this.solutionBuildManager2.AdviseUpdateSolutionEvents(buildStartEnd, out uint cookie);
                buildStartEnd.BuildEvent += BuildStartEnd_BuildEvent;
            });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
        }

        private void BuildStartEnd_BuildEvent(object sender, BuildStartEndArgs e)
        {
            if (!building)
            {
                ExternalBuildEvent?.Invoke(this, e);
            }
        }

        public async Task<bool> BuildAsync(List<IVsHierarchy> projects, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (await this.RequiresBuildAsync(cancellationToken))
            {
                return await BuildAsync(cancellationToken);
            }
            return true;
        }

        private async Task<bool> BuildAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            BuildCompletionHandler buildHandler = new BuildCompletionHandler(cancellationToken);
            int hr = solutionBuildManager2.AdviseUpdateSolutionEvents(buildHandler, out uint cookie);
            ErrorHandler.ThrowOnFailure(hr);
            bool succeeded = false;
            try
            {
                building = true;
                int result = solutionBuildManager2.StartSimpleUpdateSolutionConfiguration(
                    (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD, 0, 1);
                ErrorHandler.ThrowOnFailure(result);
                succeeded = await buildHandler.BuildCompleted;
            }
            catch (OperationCanceledException)
            {
                solutionBuildManager2.CancelUpdateSolutionConfiguration();
                throw;
            }
            finally
            {
                building = false;
                buildHandler.Dispose();
                solutionBuildManager2.UnadviseUpdateSolutionEvents(cookie);
            }
            return succeeded;
        }

        private async Task<bool> RequiresBuildAsync(CancellationToken cancellationToken)
        {
            bool respectOnlyBuildStartupProjectsAndDependenciesOnRun = false;
            if (!respectOnlyBuildStartupProjectsAndDependenciesOnRun) return true;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (solutionBuildManager3 == null)
                return true;

            int hr = solutionBuildManager3.AreProjectsUpToDate((uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD);
            return hr != VSConstants.S_OK;
        }
    }
}
