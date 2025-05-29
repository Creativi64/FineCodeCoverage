using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICoverageProjectSettingsProvider))]
    internal class CoverageProjectSettingsProvider : ICoverageProjectSettingsProvider
    {
        private readonly IVsBuildFCCSettingsProvider vsBuildFCCSettingsProvider;

        [ImportingConstructor]
        public CoverageProjectSettingsProvider(
            IVsBuildFCCSettingsProvider vsBuildFCCSettingsProvider
        ) => this.vsBuildFCCSettingsProvider = vsBuildFCCSettingsProvider;

        public async Task<XElement> ProvideAsync(ICoverageProject coverageProject)
        {
            XElement settingsElement = this.ProjectSettingsElementFromFCCLabelledPropertyGroup(coverageProject) ??
                await this.vsBuildFCCSettingsProvider.GetSettingsAsync(coverageProject.Id);
            return settingsElement;
        }

        /*
            <PropertyGroup Label="FineCodeCoverage">
            ...
            </PropertyGroup>
        */
        private XElement ProjectSettingsElementFromFCCLabelledPropertyGroup(ICoverageProject coverageProject)
            => coverageProject.ProjectFileXElement.XPathSelectElement($"/PropertyGroup[@Label='{Vsix.Code}']");
    }
}
