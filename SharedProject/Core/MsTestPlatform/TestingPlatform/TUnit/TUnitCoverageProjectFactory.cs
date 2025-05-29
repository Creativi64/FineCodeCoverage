using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverageProjectFactory))]
    internal class TUnitCoverageProjectFactory : ITUnitCoverageProjectFactory
    {
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly ITemplatedRunSettingsService templatedRunSettingsService;
        private readonly IServiceProvider serviceProvider;
        private readonly IXmlUtils xmlUtils;
        private readonly IRunSettingsToConfiguration runSettingsToConfiguration;

        class TUnitCoverageProject : ITUnitCoverageProject
        {
            private readonly Func<CancellationToken, Task<string>> configurationProvider;

            public TUnitCoverageProject(
                string exePath,
                ICoverageProject coverageProject,
                IVsHierarchy vsHierarchy,
                CommandLineParseResult commandLineParseResult,
                Func<CancellationToken, Task<string>> configurationProvider,
                bool hasCoverageExtension
            )
            {
                ExePath = exePath;
                CoverageProject = coverageProject;
                VsHierarchy = vsHierarchy;
                CommandLineParseResult = commandLineParseResult;
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
            public CommandLineParseResult CommandLineParseResult { get; }
            public bool HasCoverageExtension { get; }
        }

        [ImportingConstructor]
        public TUnitCoverageProjectFactory(
            ICoverageProjectFactory coverageProjectFactory,
            ITemplatedRunSettingsService templatedRunSettingsService,
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            IXmlUtils xmlUtils,
            IRunSettingsToConfiguration runSettingsToConfiguration
        )
        {
            this.coverageProjectFactory = coverageProjectFactory;
            this.templatedRunSettingsService = templatedRunSettingsService;
            this.serviceProvider = serviceProvider;
            this.xmlUtils = xmlUtils;
            this.runSettingsToConfiguration = runSettingsToConfiguration;
        }

        private async Task<ICoverageProject> CreateCoverageProjectAsync(
            IVsHierarchy project,
            CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ICoverageProject coverageProject = coverageProjectFactory.Create();
            project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out object projectName);
            coverageProject.ProjectName = projectName.ToString();
            project.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_CmdUIGuid, out Guid projectGuid);
            coverageProject.Id = projectGuid;
            project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out object targetFrameworkMoniker);
            cancellationToken.ThrowIfCancellationRequested();
            if (project is IVsBuildPropertyStorage buildPropertyStorage)
            {
                //todo configuration parameter for Debug
                int hr = buildPropertyStorage.GetPropertyValue("TargetPath", null, 1, out string outputFile);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.TestDllFile = outputFile;
            }//todo throw if not
            cancellationToken.ThrowIfCancellationRequested();
            if (project is IVsProject vsProject)
            {
                int hr = vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out string projectFilePath);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.ProjectFilePath = projectFilePath;
            }//todo throw if not

            return coverageProject;
        }

        private async Task<string> GetSolutionDirectoryAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            IVsSolution vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            vsSolution.GetSolutionInfo(out string solutionDirectory, out string _, out string __);
            return solutionDirectory;
        }

        private async Task<XElement> GetConfigurationElementAsync(ICoverageProject coverageProject, CancellationToken ct)
        {
            string solutionDirectory = await GetSolutionDirectoryAsync(ct);
            string runSettings = templatedRunSettingsService.CreateProjectsRunSettings(new ICoverageProject[] { coverageProject }, solutionDirectory, "")[0].RunSettings;
            return runSettingsToConfiguration.ConvertToConfiguration(XElement.Parse(runSettings));
        }

        public async Task<ITUnitCoverageProject> CreateTUnitCoverageProjectAsync(
            ITUnitProject tUnitProject,
            CancellationToken cancellationToken)
        {
            ICoverageProject coverageProject = await CreateCoverageProjectAsync(tUnitProject.Hierarchy, cancellationToken);
            string exePath = Path.ChangeExtension(coverageProject.TestDllFile, ".exe");

            Func<CancellationToken, Task<string>> configurationProvider = async (ct) =>
            {
                XElement configurationElement = await GetConfigurationElementAsync(coverageProject, ct);
                if (coverageProject.Settings.IncludeTestAssembly)
                {
                    configurationElement.Add(new XElement("IncludeTestAssembly", true));
                }
                return xmlUtils.Serialize(configurationElement);
            };

            return new TUnitCoverageProject(
                exePath,
                coverageProject,
                tUnitProject.Hierarchy,
                tUnitProject.CommandLineParseResult,
                configurationProvider,
                tUnitProject.HasCoverageExtension);
        }
    }

}
