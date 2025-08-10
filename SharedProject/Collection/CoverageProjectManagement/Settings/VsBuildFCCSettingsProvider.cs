using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    [Export(typeof(IVsBuildFCCSettingsProvider))]
    internal sealed class VsBuildFCCSettingsProvider : IVsBuildFCCSettingsProvider
    {
        private const string FCCSettingsElementName = "FineCodeCoverage";
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public VsBuildFCCSettingsProvider(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task<XElement> GetSettingsAsync(Guid projectId)
        {
            XElement fccSettingsElement = null;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            if (vsSolution.GetProjectOfGuid(ref projectId, out IVsHierarchy vsHierarchy) == VSConstants.S_OK
                && vsHierarchy is IVsBuildPropertyStorage vsBuildPropertyStorage
                && vsBuildPropertyStorage.GetPropertyValue(FCCSettingsElementName, null, 1, out string value) == VSConstants.S_OK
                && !string.IsNullOrEmpty(value))
            {
                try
                {
                    fccSettingsElement = XElement.Parse($"<FineCodeCoverage>{value}</FineCodeCoverage>");
                }
                catch
                {
                }
            }

            return fccSettingsElement;
        }
    }
}
