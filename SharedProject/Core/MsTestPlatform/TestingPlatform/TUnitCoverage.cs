using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using Microsoft.CodeAnalysis;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;


namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal static class IVsHierarchyExtensions
    {
        public static EnvDTE.Project ToProject(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Retrieve the automation object from the root of the hierarchy.
            int hr = hierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out object extObject);

            if (ErrorHandler.Succeeded(hr) && extObject is EnvDTE.Project project)
            {
                return project;
            }

            return null;
        }

        public static Guid GetGuid(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int hr = hierarchy.GetGuidProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ProjectIDGuid,
                out Guid projectGuid);

            return projectGuid;
        }

    }
    internal interface ITUnitProjectsProvider
    {
        Task<List<IVsHierarchy>> GetTUnitProjectsWithCoverageExtensionAsync();
    }

    [Export(typeof(ITUnitProjectsProvider))]
    internal class TUnitProjectsProvider : ITUnitProjectsProvider
    {
        private readonly IServiceProvider serviceProvider;
        private readonly AsyncLazy<INuGetProjectService> lazyNugetProjectService;

        [ImportingConstructor]
        public TUnitProjectsProvider(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            AsyncServiceProviderProvider asyncServiceProviderProvider
        ) {
            this.serviceProvider = serviceProvider;
            lazyNugetProjectService = new AsyncLazy<INuGetProjectService>(async () =>
            {
                var brokeredServiceContainer = await asyncServiceProviderProvider.Provider.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
                IServiceBroker serviceBroker = brokeredServiceContainer.GetFullAccessServiceBroker();
#pragma warning disable ISB001 // Dispose of proxies
                INuGetProjectService nugetProjectService = await serviceBroker.GetProxyAsync<INuGetProjectService>(NuGetServices.NuGetProjectServiceV1);
#pragma warning restore ISB001 // Dispose of proxies
                return nugetProjectService;
            }, ThreadHelper.JoinableTaskFactory);
        }
        public async Task<List<IVsHierarchy>> GetTUnitProjectsWithCoverageExtensionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            var result = vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION,Guid.Empty, out var enumHierarchies);
            if(result == VSConstants.S_OK)
            {
                return await GetApplicableProjectsAsync(enumHierarchies);
            }
            return Enumerable.Empty<IVsHierarchy>().ToList();
        }
        private async Task<List<IVsHierarchy>> GetApplicableProjectsAsync(IEnumHierarchies vsEnumHierarchies)
        {
            List<IVsHierarchy> applicableProjects = new List<IVsHierarchy>();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchy[] rgelt = new IVsHierarchy[1];
            uint fetched = 0;
            while (vsEnumHierarchies.Next(1, rgelt, out fetched) == VSConstants.S_OK && fetched > 0)
            {
                IVsHierarchy projectHierarchy = rgelt[0];
                if (await IsApplicableProjectAsync(projectHierarchy))
                {
                    applicableProjects.Add(projectHierarchy);
                }
            }
            return applicableProjects;
        }

        private async Task<bool> IsApplicableProjectAsync(IVsHierarchy project) {
            /*
                could have used ?
                from TUnit.Engine.props
                <IsTestProject>true</IsTestProject>
            */
            return project.IsCapabilityMatch("TestContainer") && await IsTUnitWithCoverageExtensionAsync(project);
        }

        private async Task<bool> IsTUnitWithCoverageExtensionAsync(IVsHierarchy vsHierarchy) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var isTUnitWithCoverageExtension = false;
            var nugetProjectService = await lazyNugetProjectService.GetValueAsync();
            // might throw ?
            var installedPackagesResult = await nugetProjectService.GetInstalledPackagesAsync(vsHierarchy.GetGuid(), CancellationToken.None);
            if (installedPackagesResult.Status == InstalledPackageResultStatus.Successful)
            {
                var hasTUnit = false;
                var hasCoverageExtension = false;
                foreach (var package in installedPackagesResult.Packages)
                {
                    var id = package.Id;
                    if (id == "TUnit")
                    {
                        hasTUnit = true;
                        continue;
                    }
                    if (id == "Microsoft.Testing.Extensions.CodeCoverage")
                    {
                        hasCoverageExtension = true;
                    }
                }
                isTUnitWithCoverageExtension = hasTUnit && hasCoverageExtension;
            }
            return isTUnitWithCoverageExtension;
        }
    }

    internal interface IBuildHelper
    {
        Task<bool> BuildInDebugConfigAsync(List<IVsHierarchy> projects);
    }

    public static class ProjectDependencyHelper
    {
        /// <summary>
        /// Gets the transitive closure of project dependencies.
        /// </summary>
        /// <param name="buildManager">The IVsSolutionBuildManager2 instance.</param>
        /// <param name="projects">The projects for which dependencies are required.</param>
        /// <returns>A list of IVsHierarchy instances representing all transitive dependencies.</returns>
        public static List<IVsHierarchy> GetTransitiveDependencies(IVsSolutionBuildManager2 buildManager, IEnumerable<IVsHierarchy> projects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Ensure the dependency information is calculated.
            int hr = buildManager.CalculateProjectDependencies();
            ErrorHandler.ThrowOnFailure(hr);

            var collected = new HashSet<IVsHierarchy>(new VsHierarchyComparer());
            foreach(var project in projects)
            {
                AddDependencies(buildManager, project, collected);
            }
            return collected.ToList();
        }

        /// <summary>
        /// Recursively adds direct dependencies to the collected set.
        /// </summary>
        private static void AddDependencies(IVsSolutionBuildManager2 buildManager, IVsHierarchy project, HashSet<IVsHierarchy> collected)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // First call: Get the count of direct dependencies.
            uint[] actualCount = new uint[1];
            int hr = buildManager.GetProjectDependencies(project, 0, null, actualCount);
            ErrorHandler.ThrowOnFailure(hr);
            uint dependencyCount = actualCount[0];

            if (dependencyCount == 0)
            {
                return;
            }

            // Allocate an array for the dependencies and call again to fill it.
            IVsHierarchy[] directDependencies = new IVsHierarchy[dependencyCount];
            uint[] actualCount2 = new uint[1];
            hr = buildManager.GetProjectDependencies(project, dependencyCount, directDependencies, actualCount2);
            ErrorHandler.ThrowOnFailure(hr);

            // Process each direct dependency recursively.
            foreach (var dep in directDependencies)
            {
                if (dep != null && collected.Add(dep))
                {
                    AddDependencies(buildManager, dep, collected);
                }
            }
        }
    }

    /// <summary>
    /// Compares IVsHierarchy objects by comparing their project GUIDs.
    /// </summary>
    public class VsHierarchyComparer : IEqualityComparer<IVsHierarchy>
    {
        public bool Equals(IVsHierarchy x, IVsHierarchy y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.GetGuid().Equals(y.GetGuid());
        }

        public int GetHashCode(IVsHierarchy obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetGuid().GetHashCode();
        }
    }

    public class BuildCompletionHandler : IVsUpdateSolutionEvents
    {
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

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
        public async Task<bool> BuildInDebugConfigAsync(List<IVsHierarchy> projects)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solutionBuildManager2 = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            var dependencies = ProjectDependencyHelper.GetTransitiveDependencies(solutionBuildManager2, projects);
            var projectsToBuild = projects.Concat(dependencies).ToArray();
            //todo
            IVsCfg[] configs = new IVsCfg[projectsToBuild.Length];
            var buildHandler = new BuildCompletionHandler();
            int hr = solutionBuildManager2.AdviseUpdateSolutionEvents(buildHandler, out uint cookie);
            ErrorHandler.ThrowOnFailure(hr);
            var succeeded = false;
            try
            {
                var result = solutionBuildManager2.StartUpdateSpecificProjectConfigurations(
                    (uint)projectsToBuild.Length,
                    projectsToBuild,
                    projectsToBuild.Select(GetDebugConfig).ToArray(),
                    null,
                    null,
                    null,
                    (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD,
                    1
                    );
                ErrorHandler.ThrowOnFailure(result);
                succeeded = await buildHandler.BuildCompleted;
            }
            finally
            {
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

    internal interface ITUnitCoverageProject
    {
        string ExePath { get; }
        string Configuration { get; }
        ICoverageProject CoverageProject { get; }
    }
    internal interface ITUnitCoverageProjectFactory
    {
        Task<ITUnitCoverageProject> CreateCoverageProjectAsync(IVsHierarchy project);
    }

    [Export(typeof(ITUnitCoverageProjectFactory))]
    internal class TUnitCoverageProjectFactory : ITUnitCoverageProjectFactory
    {
        private readonly ICoverageProjectFactory coverageProjectFactory;

        class TUnitCoverageProject : ITUnitCoverageProject
        {
            public TUnitCoverageProject(string exePath, string configuration, ICoverageProject coverageProject)
            {
                ExePath = exePath;
                Configuration = configuration;
                CoverageProject = coverageProject;
            }
            public string ExePath { get; }
            public string Configuration { get; }
            public ICoverageProject CoverageProject { get; }
        }

        [ImportingConstructor]
        public TUnitCoverageProjectFactory(
            ICoverageProjectFactory coverageProjectFactory
        )
        {
            this.coverageProjectFactory = coverageProjectFactory;
        }
        public async Task<ITUnitCoverageProject> CreateCoverageProjectAsync(IVsHierarchy project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var coverageProject = coverageProjectFactory.Create();
            project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out var projectName);
            coverageProject.ProjectName = projectName.ToString();
            project.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_CmdUIGuid, out var projectGuid);
            coverageProject.Id = projectGuid;
            project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out var targetFrameworkMoniker);

            if (project is IVsBuildPropertyStorage buildPropertyStorage)
            {
                //todo configuration parameter for Debug
                int hr = buildPropertyStorage.GetPropertyValue("TargetPath", null, 1, out var outputFile);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.TestDllFile = outputFile;
            }//todo throw if not

            if(project is IVsProject vsProject)
            {
                int hr = vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out var projectFilePath);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.ProjectFile = projectFilePath;
            }//todo throw if not

            var exePath = Path.ChangeExtension(coverageProject.TestDllFile,".exe");

            //todo configuration
            return new TUnitCoverageProject(exePath, "", coverageProject);
        }
    }

    internal interface ITUnitRunner
    {
        // todo change to return exit code too for logging
        Task<bool> RunAsync(string exePath, string settingsPath, string outputpath);
    }

    [Export(typeof(ITUnitRunner))]
    internal class TUnitRunner : ITUnitRunner
    {
        public async Task<bool> RunAsync(string exePath, string settingsPath, string outputpath)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = exePath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Arguments = $"--coverage --coverage-output-format cobertura --coverage-output \"{outputpath}\"";
                // Optionally set other start info properties (e.g. arguments, CreateNoWindow, etc.)
                process.Start();
                //todo cancellation token
                await process.WaitForExitAsync();

                /*
                    from https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro?tabs=dotnetcli#run-and-debug-tests
                	The app exits with a nonzero exit code if there's an error, which is typical for most executables. For more information on the known exit codes, see Microsoft.Testing.Platform exit codes.
					Tip
				    You can ignore a specific exit code using the --ignore-exit-code command line option.

                */
                return process.ExitCode == 0;
            }
        }
    }


    [Export(typeof(ITUnitCoverage))]
    internal class TUnitCoverage : ITUnitCoverage
    {
        private readonly ITUnitProjectsProvider tUnitProjectsProvider;
        private readonly IBuildHelper buildHelper;
        private readonly ITUnitCoverageProjectFactory tUnitCoverageProjectFactory;
        private readonly ITUnitRunner tUnitRunner;
        private readonly ICoverageToolOutputManager coverageToolOutputManager;
        private readonly IFCCEngine fccEngine;
        private readonly IFileUtil fileUtil;

        [ImportingConstructor]
        public TUnitCoverage(
            ITUnitProjectsProvider tUnitProjectsProvider,
            IBuildHelper buildHelper,
            ITUnitCoverageProjectFactory tUnitCoverageProjectFactory,
            ITUnitRunner tUnitRunner,
            ICoverageToolOutputManager coverageToolOutputManager,
            IFCCEngine fccEngine,
            IFileUtil fileUtil

        )
        {
            this.tUnitProjectsProvider = tUnitProjectsProvider;
            this.buildHelper = buildHelper;
            this.tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            this.tUnitRunner = tUnitRunner;
            this.coverageToolOutputManager = coverageToolOutputManager;
            this.fccEngine = fccEngine;
            this.fileUtil = fileUtil;
        }
        public void CollectCoverage()
        {
            _ = Task.Run(async () => await CollectCoverageAsync());
        }

        private async Task CollectCoverageAsync()
        {
            var tUnitProjects = await tUnitProjectsProvider.GetTUnitProjectsWithCoverageExtensionAsync();
            if (tUnitProjects.Any())
            {
                var buildSuccess = await buildHelper.BuildInDebugConfigAsync(tUnitProjects);
                if (buildSuccess)
                {
                    var tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => tUnitCoverageProjectFactory.CreateCoverageProjectAsync(tUnitProject)));
                    var coverageProjects = tUnitCoverageProjects.Select(tUnitCoverageProject => tUnitCoverageProject.CoverageProject).ToList();
                    var runAllProjects = true;
                    List<string> coberturaFiles = new List<string>();
                    await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
                    foreach (var tUnitCoverageProject in tUnitCoverageProjects)
                    {
                        var coverageProject = tUnitCoverageProject.CoverageProject;
                        await coverageProject.PrepareForCoverageAsync(CancellationToken.None, false);
                        var configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
                        //fileUtil.WriteAllText(configurationPath, tUnitCoverageProject.Configuration);                        
                        var coberturaPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
                        await Task.Yield();//todo was this how to get off ui thread ?
                        var success = await tUnitRunner.RunAsync(tUnitCoverageProject.ExePath, configurationPath, coberturaPath);
                        if (success)
                        {
                            coberturaFiles.Add(coberturaPath);
                        }
                        else
                        {
                            // show message box
                            runAllProjects = false;
                        }
                    }
                    if (runAllProjects)
                    {
                        fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
                    }
                }
                else
                {
                    //todo - show a message box ?
                }
            }
            else
            {
                // todo - show a message box ?
            }

        }
    }
}
