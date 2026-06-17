using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Utilities.Files;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.Ms
{
    [Export(typeof(IVsRunSettingsWriter))]
    internal sealed class VsRunSettingsWriter : IVsRunSettingsWriter
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
            IProjectFilePropertyWriter projectFilePropertyWriter)
        {
            _serviceProvider = serviceProvider;
            _projectSaver = projectSaver;
            _projectFilePropertyWriter = projectFilePropertyWriter;
        }

        public async Task<bool> RemoveRunSettingsFilePathAsync(Guid projectGuid)
        {
            bool ok = false;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            if (vsSolution.GetProjectOfGuid(ref projectGuid, out IVsHierarchy vsHierarchy) == VSConstants.S_OK)
            {
                ok = await _projectFilePropertyWriter.RemovePropertyAsync(vsHierarchy, ProjectRunSettingsFilePathElementName);
                if (ok)
                {
                    await _projectSaver.SaveProjectAsync(vsHierarchy);
                }
            }

            return ok;
        }
    }
}
