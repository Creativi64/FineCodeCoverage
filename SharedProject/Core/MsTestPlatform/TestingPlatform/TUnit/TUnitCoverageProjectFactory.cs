using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using System.Xml.Linq;
using System.Linq;
using System;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverageProjectFactory))]
    internal class TUnitCoverageProjectFactory : ITUnitCoverageProjectFactory
    {
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly ITemplatedRunSettingsService templatedRunSettingsService;
        private readonly IServiceProvider serviceProvider;

        class TUnitCoverageProject : ITUnitCoverageProject
        {
            private readonly Func<CancellationToken,Task<string>> configurationProvider;

            public TUnitCoverageProject(
                string exePath,
                ICoverageProject coverageProject,
                IVsHierarchy vsHierarchy,
                Func<CancellationToken, Task<string>> configurationProvider,
                bool hasCoverageExtension
            )
            {
                ExePath = exePath;
                CoverageProject = coverageProject;
                VsHierarchy = vsHierarchy;
                this.configurationProvider = configurationProvider;
                HasCoverageExtension = hasCoverageExtension;
            }
            public string ExePath { get; }
            public Task<string> GetConfigurationAsync(CancellationToken cancellationToken)
            {
                return configurationProvider(cancellationToken);
            }
            public ICoverageProject CoverageProject { get; }
            public IVsHierarchy VsHierarchy { get; }
            public bool HasCoverageExtension { get; }
        }

        [ImportingConstructor]
        public TUnitCoverageProjectFactory(
            ICoverageProjectFactory coverageProjectFactory,
            ITemplatedRunSettingsService templatedRunSettingsService,
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            this.coverageProjectFactory = coverageProjectFactory;
            this.templatedRunSettingsService = templatedRunSettingsService;
            this.serviceProvider = serviceProvider;
        }
        public async Task<ITUnitCoverageProject> CreateCoverageProjectAsync(
            IVsHierarchy project,
            bool hasCoverageExtension,
            CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var coverageProject = coverageProjectFactory.Create();
            project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out var projectName);
            coverageProject.ProjectName = projectName.ToString();
            project.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_CmdUIGuid, out var projectGuid);
            coverageProject.Id = projectGuid;
            project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out var targetFrameworkMoniker);
            cancellationToken.ThrowIfCancellationRequested();
            if (project is IVsBuildPropertyStorage buildPropertyStorage)
            {
                //todo configuration parameter for Debug
                int hr = buildPropertyStorage.GetPropertyValue("TargetPath", null, 1, out var outputFile);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.TestDllFile = outputFile;
            }//todo throw if not
            cancellationToken.ThrowIfCancellationRequested();
            if (project is IVsProject vsProject)
            {
                int hr = vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out var projectFilePath);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.ProjectFile = projectFilePath;
            }//todo throw if not

            var exePath = Path.ChangeExtension(coverageProject.TestDllFile, ".exe");

            var vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            vsSolution.GetSolutionInfo(out string solutionDirectory, out var _, out var __);

            Func<CancellationToken,Task<string>> configurationProvider = async (ct) =>
            {
                await coverageProject.PrepareForCoverageAsync(ct,false);
                var runSettings = templatedRunSettingsService.CreateProjectsRunSettings(new ICoverageProject[] { coverageProject }, solutionDirectory, "")[0].RunSettings;
                var configurationElement = XElement.Parse(runSettings).Descendants("Configuration").First();
                if (coverageProject.Settings.IncludeTestAssembly)
                {
                    configurationElement.Add(new XElement("IncludeTestAssembly", true));
                }
                return configurationElement.ToString();
            };

            return new TUnitCoverageProject(exePath,coverageProject, project, configurationProvider, hasCoverageExtension);
        }
    }

}
