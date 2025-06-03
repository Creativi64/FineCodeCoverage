using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IResourceProvider))]
    internal class ResourceProvider : IResourceProvider
    {
        private readonly string resourcesDirectory;
        public ResourceProvider()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.resourcesDirectory = Path.Combine(assemblyDirectory, "Resources");
        }

        public string ReadResource(string resourceName)
            => File.ReadAllText(Path.Combine(this.resourcesDirectory, resourceName));
    }
}