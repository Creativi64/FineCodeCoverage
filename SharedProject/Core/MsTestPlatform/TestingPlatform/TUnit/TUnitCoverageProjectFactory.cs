using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
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

            if (project is IVsProject vsProject)
            {
                int hr = vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out var projectFilePath);
                ErrorHandler.ThrowOnFailure(hr);
                coverageProject.ProjectFile = projectFilePath;
            }//todo throw if not

            var exePath = Path.ChangeExtension(coverageProject.TestDllFile, ".exe");

            //todo configuration
            return new TUnitCoverageProject(exePath, "", coverageProject);
        }
    }

}
