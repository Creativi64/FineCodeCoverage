using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using Microsoft.VisualStudio.Shell;
using VSLangProj;
using VSLangProj80;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(IDotNetReferencedProjectsHelper))]
    internal sealed class DotNetReferencedProjectsHelper : IDotNetReferencedProjectsHelper
    {
        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(VSProject vsProject)
        {
            List<ReferencedProject> referencedProjects = (await System.Threading.Tasks.Task.WhenAll(GetReferencedSourceProjects(vsProject).Select(GetReferencedProjectAsync))).ToList();
            return new List<IExcludableReferencedProject>(referencedProjects);
        }

        private static IEnumerable<Project> GetReferencedSourceProjects(VSProject vsproject)
            => vsproject.References.Cast<Reference>().Where(r => r.SourceProject != null)
                .Select(r => r.SourceProject);

        private async Task<ReferencedProject> GetReferencedProjectAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            (string assemblyName, bool isDll) = await GetAssemblyNameIsDllAsync(project);
            return new ReferencedProject(project.FullName, assemblyName, isDll);
        }

        private static async Task<(string, bool)> GetAssemblyNameIsDllAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Property assemblyNameProperty = project.Properties.Item(nameof(ProjectProperties3.AssemblyName));
            string assemblyName = assemblyNameProperty?.Value.ToString() ?? project.Name;
            Property outputTypeProperty = project.Properties.Item(nameof(ProjectProperties3.OutputType));
            bool isDll = true;
            if (outputTypeProperty != null)
            {
                var po = (prjOutputType)Enum.Parse(typeof(prjOutputType), outputTypeProperty.Value.ToString());
                isDll = po == prjOutputType.prjOutputTypeLibrary;
            }

            return (assemblyName, isDll);
        }
    }
}
