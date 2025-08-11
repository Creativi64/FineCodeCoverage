using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Readme.Template
{
    [Export(typeof(ITemplatedReadmeProvider))]
    internal sealed class TemplatedReadmeProvider : ITemplatedReadmeProvider
    {
        public TemplatedReadmeInfo GetTemplatedReadme()
        {
            FileInfo readmeFile = GetReadMeTemplateFile();
            return new TemplatedReadmeInfo(
                File.ReadAllText(readmeFile.FullName),
                readmeFile.Directory.FullName);
        }

        private static FileInfo GetReadMeTemplateFile()
        {
            DirectoryInfo dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            return dir.EnumerateFiles("README-Template.md", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
