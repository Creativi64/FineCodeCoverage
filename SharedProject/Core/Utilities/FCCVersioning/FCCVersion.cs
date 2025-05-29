using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace FineCodeCoverage.Core.Utilities.FCCVersioning
{
    [Export(typeof(IFCCVersion))]
    internal class FCCVersion : IFCCVersion
    {
        private string version;
        [ImportingConstructor]
        public FCCVersion()
        {

        }
        public string GetVersion()
        {
            if (this.version == null)
            {
                this.SetVsixVersion();
            }

            return this.version;
        }

        private void SetVsixVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            string asmDir = Path.GetDirectoryName(asm.Location);
            string manifestPath = Path.Combine(asmDir, "extension.vsixmanifest");
            if (File.Exists(manifestPath))
            {
                var doc = new XmlDocument();
                doc.Load(manifestPath);
                XmlElement metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Metadata");
                XmlElement identity = metaData.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Identity");
                this.version = identity.GetAttribute("Version");

            }
        }
    }

}
