using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    [Export(typeof(ICoverageProjectSettingsProvider))]
    internal sealed class CoverageProjectSettingsProvider : ICoverageProjectSettingsProvider
    {
        private readonly IVsBuildFCCSettingsProvider _vsBuildFCCSettingsProvider;

        [ImportingConstructor]
        public CoverageProjectSettingsProvider(
            IVsBuildFCCSettingsProvider vsBuildFCCSettingsProvider)
            => _vsBuildFCCSettingsProvider = vsBuildFCCSettingsProvider;

        public async Task<XElement> ProvideAsync(ICoverageProject coverageProject)
            => ProjectSettingsElementFromFCCLabelledPropertyGroup(coverageProject) ??
                await _vsBuildFCCSettingsProvider.GetSettingsAsync(coverageProject.Id);

        /*
            <PropertyGroup Label="FineCodeCoverage">
            ...
            </PropertyGroup>
        */
        private static XElement ProjectSettingsElementFromFCCLabelledPropertyGroup(
            ICoverageProject coverageProject) => coverageProject.ProjectFileXElement.XPathSelectElement($"/PropertyGroup[@Label='FineCodeCoverage']");
    }
}
