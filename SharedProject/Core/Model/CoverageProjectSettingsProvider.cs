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
            => ProjectSettingsElementFromFCCLabelledPropertyGroup(coverageProject) ??
                await this.vsBuildFCCSettingsProvider.GetSettingsAsync(coverageProject.Id);

        /*
            <PropertyGroup Label="FineCodeCoverage">
            ...
            </PropertyGroup>
        */
        private static XElement ProjectSettingsElementFromFCCLabelledPropertyGroup(
            ICoverageProject coverageProject
        ) => coverageProject.ProjectFileXElement.XPathSelectElement($"/PropertyGroup[@Label='{Vsix.Code}']");
    }
}
