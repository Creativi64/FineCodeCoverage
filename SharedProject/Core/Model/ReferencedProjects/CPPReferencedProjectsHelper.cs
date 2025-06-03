using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICPPReferencedProjectsHelper))]
    internal class CPPReferencedProjectsHelper : ICPPReferencedProjectsHelper
    {
        private static VCProject GetReferencedVCProject(VCProjectReference projectReference)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return projectReference.ReferencedProject as VCProject
                ?? (projectReference.ReferencedProject as EnvDTE.Project)?.Object as VCProject;
        }

        private static bool? IsDll(VCProject vcProject)
        {
            if (!(vcProject.Configurations is IEnumerable configurations))
                return null;

            VCConfiguration configuration = configurations.Cast<VCConfiguration>().FirstOrDefault();
            if (configuration == null)
                return null;

            bool isDll = configuration.ConfigurationType == ConfigurationTypes.typeDynamicLibrary;
            bool isApplication = configuration.ConfigurationType == ConfigurationTypes.typeApplication;
            return !isDll && !isApplication ? null : (bool?)isDll;
        }

        private static string GetCPPProjectReferenceProjectFilePath(VCProjectReference reference)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vsReference = reference.Reference as VSLangProj.Reference;
            EnvDTE.Project sourceProject = vsReference.SourceProject;
            return sourceProject.FileName;
        }

        public async Task<List<IExcludableReferencedProject>> GetInstrumentableReferencedProjectsAsync(VCProject cppProject)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            return !(cppProject.VCReferences is IEnumerable vcReferences)
                ? null
                : vcReferences
                .OfType<VCProjectReference>()
                .Select(reference =>
                {
                    VCProject referencedProject = GetReferencedVCProject(reference);

                    bool? isDll = IsDll(referencedProject);
                    return isDll.HasValue ? (IExcludableReferencedProject)new ReferencedProject(
                            GetCPPProjectReferenceProjectFilePath(reference),
                            Path.GetFileNameWithoutExtension(reference.FullPath),
                            isDll.Value
                        )
                        : null;
                })
                .Where(p => p != null)
                .ToList();
        }
    }
}