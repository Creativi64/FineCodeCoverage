using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Core.Utilities;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IVsRunSettingsWriter))]
    internal class VsRunSettingsWriter : IVsRunSettingsWriter
    {
        private const string ProjectRunSettingsFilePathElementName = "RunSettingsFilePath";
        private readonly IServiceProvider _serviceProvider;
        private readonly IProjectSaver _projectSaver;
        private readonly IProjectFilePropertyWriter _projectFilePropertyWriter;

        [ImportingConstructor]
        public VsRunSettingsWriter(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            IProjectSaver projectSaver,
            IProjectFilePropertyWriter projectFilePropertyWriter
        )
        {
            this._serviceProvider = serviceProvider;
            this._projectSaver = projectSaver;
            this._projectFilePropertyWriter = projectFilePropertyWriter;
        }

        public async Task<bool> WriteRunSettingsFilePathAsync(Guid projectGuid, string projectRunSettingsFilePath)
        {
            bool success = false;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = this._serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            if (vsSolution.GetProjectOfGuid(ref projectGuid, out IVsHierarchy vsHierarchy) == VSConstants.S_OK)
            {
                success = await this._projectFilePropertyWriter.WritePropertyAsync(vsHierarchy, ProjectRunSettingsFilePathElementName, projectRunSettingsFilePath);
            }

            return success;
        }

        public async Task<bool> RemoveRunSettingsFilePathAsync(Guid projectGuid)
        {

            bool ok = false;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = this._serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            if (vsSolution.GetProjectOfGuid(ref projectGuid, out IVsHierarchy vsHierarchy) == VSConstants.S_OK)
            {
                ok = await this._projectFilePropertyWriter.RemovePropertyAsync(vsHierarchy, ProjectRunSettingsFilePathElementName);
                if (ok)
                {
                    await this._projectSaver.SaveProjectAsync(vsHierarchy);
                }
            }

            return ok;
        }
    }
}