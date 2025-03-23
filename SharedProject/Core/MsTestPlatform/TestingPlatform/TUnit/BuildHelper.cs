using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
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

    [Export(typeof(IBuildHelper))]
    internal class BuildHelper : IBuildHelper
    {
        private readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public BuildHelper(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }
        public async Task<bool> BuildAsync(List<IVsHierarchy> projects,CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var solutionBuildManager2 = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            var dependencies = await ProjectDependencyHelper.GetTransitiveDependenciesAsync(solutionBuildManager2, projects, cancellationToken);
            var projectsToBuild = projects.Concat(dependencies).ToArray();
            cancellationToken.ThrowIfCancellationRequested();
            var configs = projectsToBuild.Select(GetDebugConfig).ToArray();
            cancellationToken.ThrowIfCancellationRequested();

            var buildHandler = new BuildCompletionHandler(cancellationToken);
            int hr = solutionBuildManager2.AdviseUpdateSolutionEvents(buildHandler, out uint cookie);
            ErrorHandler.ThrowOnFailure(hr);
            var succeeded = false;
            try
            {
                var result = solutionBuildManager2.StartUpdateSpecificProjectConfigurations(
                    (uint)projectsToBuild.Length,
                    projectsToBuild,
                    configs,
                    null,
                    null,
                    null,
                    (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD,
                    1
                    );
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
                buildHandler.Dispose();
                solutionBuildManager2.UnadviseUpdateSolutionEvents(cookie);
            }
            return succeeded;
        }

        private IVsCfg GetDebugConfig(IVsHierarchy projectHierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var cfgProvider = VsShellUtilities.GetCfgProvider(projectHierarchy);
            var configs = GetAllConfigurations(cfgProvider);
            //todo no debug config ???
            var debugConfig = configs.First(config =>
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                config.get_DisplayName(out var displayName);
                return displayName.IndexOf("debug", StringComparison.OrdinalIgnoreCase) >= 0;
            });
            return debugConfig;
        }

        public static IVsCfg[] GetAllConfigurations(IVsCfgProvider cfgProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // First call: request the count of configurations.
            uint[] actualCount = new uint[1];
            var hr = cfgProvider.GetCfgs(0, null, actualCount, null);
            ErrorHandler.ThrowOnFailure(hr);

            uint configCount = actualCount[0];
            if (configCount == 0)
                return new IVsCfg[0];

            // Allocate arrays for the configurations and their flags.
            IVsCfg[] configurations = new IVsCfg[configCount];
            uint[] flags = new uint[configCount];

            // Second call: retrieve the configurations.
            hr = cfgProvider.GetCfgs(configCount, configurations, actualCount, flags);
            ErrorHandler.ThrowOnFailure(hr);

            // Optionally, you can examine the flags array if needed.
            return configurations;
        }
    }

}
