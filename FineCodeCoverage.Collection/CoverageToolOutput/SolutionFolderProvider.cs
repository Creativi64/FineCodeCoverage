using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace FineCodeCoverage.Collection.CoverageToolOutput
{
    [Export(typeof(ISolutionFolderProvider))]
    internal sealed class SolutionFolderProvider : ISolutionFolderProvider
    {
        public string Provide(string projectFile)
        {
            string provided = null;
            DirectoryInfo directory = new FileInfo(projectFile).Directory;
            while (directory != null)
            {
                bool isSolutionDirectory = directory.EnumerateFiles().Any(IsSolutionFile);
                if (isSolutionDirectory)
                {
                    provided = directory.FullName;
                    break;
                }

                directory = directory.Parent;
            }

            return provided;
        }

        private bool IsSolutionFile(FileInfo fileInfo)
            => fileInfo.Name.EndsWith(".sln") || fileInfo.Name.EndsWith(".slnx");
    }
}
