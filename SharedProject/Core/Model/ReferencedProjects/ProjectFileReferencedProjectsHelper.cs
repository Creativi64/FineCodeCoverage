using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Model
{
    // todo - remove this ? Should not be necessary
    [Export(typeof(IProjectFileReferencedProjectsHelper))]
    internal class ProjectFileReferencedProjectsHelper : IProjectFileReferencedProjectsHelper

    {
        private readonly ILogger logger;

        [ImportingConstructor]
        public ProjectFileReferencedProjectsHelper(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(
            string projectFile, XElement projectFileXElement
        )
        {
            /*
			<ItemGroup>
				<ProjectReference Include="..\BranchCoverage\Branch_Coverage.csproj" />
				<ProjectReference Include="..\FxClassLibrary1\FxClassLibrary1.csproj"></ProjectReference>
			</ItemGroup>
			 */

            var xprojectReferences = projectFileXElement.XPathSelectElements("/ItemGroup/ProjectReference[@Include]");
            List<string> referencedProjectFiles = new List<string>();
            foreach (var xprojectReference in xprojectReferences)
            {
                var referencedProjectProjectFile = xprojectReference.Attribute("Include").Value;
                if (referencedProjectProjectFile.Contains("$("))
                {
                    await logger.LogAsync($"Cannot exclude referenced project {referencedProjectProjectFile} of {projectFile} with {ReferencedProject.excludeFromCodeCoveragePropertyName}.  Cannot use MSBuildWorkspace");
                }
                else
                {
                    if (!Path.IsPathRooted(referencedProjectProjectFile))
                    {
                        referencedProjectProjectFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile), referencedProjectProjectFile));
                    }
                    referencedProjectFiles.Add(referencedProjectProjectFile);
                }

            }

            return referencedProjectFiles.ConvertAll(referencedProjectProjectFile => (IExcludableReferencedProject)new ReferencedProject(referencedProjectProjectFile));
        }
    }
}
