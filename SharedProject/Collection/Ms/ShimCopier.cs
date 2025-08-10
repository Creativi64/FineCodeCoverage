using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Collection.Ms
{
    [Export(typeof(IShimCopier))]
    internal sealed class ShimCopier : IShimCopier
    {
        private readonly IFileUtil _fileUtil;

        [ImportingConstructor]
        public ShimCopier(IFileUtil fileUtil) => _fileUtil = fileUtil;

        private void CopyShim(string shimPath, string outputFolder)
        {
            string destination = Path.Combine(outputFolder, Path.GetFileName(shimPath));
            if (_fileUtil.Exists(destination))
            {
                return;
            }

            _fileUtil.Copy(shimPath, destination);
        }

        private void CopyShim(string shimPath, IEnumerable<ICoverageProject> coverageProjects)
        {
            foreach (ICoverageProject coverageProject in coverageProjects)
            {
                CopyShim(shimPath, coverageProject.ProjectOutputFolder);
            }
        }

        public void Copy(string shimPath, IEnumerable<ICoverageProject> coverageProjects)
        {
            IEnumerable<ICoverageProject> netFrameworkCoverageProjects = coverageProjects.Where(cp => cp.IsDotNetFramework);
            CopyShim(shimPath, netFrameworkCoverageProjects);
        }
    }
}
