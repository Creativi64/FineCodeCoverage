using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IResourceProvider))]
    internal class ResourceProvider : IResourceProvider
    {
        private readonly string _resourcesDirectory;
        public ResourceProvider()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this._resourcesDirectory = Path.Combine(assemblyDirectory, "Resources");
        }

        public string ReadResource(string resourceName)
            => File.ReadAllText(Path.Combine(this._resourcesDirectory, resourceName));
    }
}
