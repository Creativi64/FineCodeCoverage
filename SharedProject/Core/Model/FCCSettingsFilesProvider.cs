using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Linq;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(IFCCSettingsFilesProvider))]
    internal class FCCSettingsFilesProvider : IFCCSettingsFilesProvider
    {
        internal const string fccOptionsFileName = "finecodecoverage-settings.xml";
        private const string topLevelAttributeName = "topLevel";
        private readonly IFileUtil fileUtil;

        [ImportingConstructor]
        public FCCSettingsFilesProvider(
            IFileUtil fileUtil
        ) => this.fileUtil = fileUtil;

        public List<XElement> Provide(string projectDirectoryPath)
        {
            var fccOptionsElements = new List<XElement>();
            string directoryPath = projectDirectoryPath;
            bool ascend = true;
            while (ascend)
            {
                ascend = this.AddFromDirectory(fccOptionsElements, directoryPath);
                if (ascend)
                {
                    directoryPath = this.fileUtil.DirectoryParentPath(directoryPath);
                    if (directoryPath == null)
                    {
                        ascend = false;
                    }
                }
            }

            fccOptionsElements.Reverse();
            return fccOptionsElements;

        }

        private bool AddFromDirectory(List<XElement> fccOptionsElements, string directory)
        {
            bool ascend = true;
            string fccOptionsPath = this.GetFCCOptionsPath(directory);
            if (this.fileUtil.Exists(fccOptionsPath))
            {
                string fccOptions = this.fileUtil.ReadAllText(fccOptionsPath);
                try
                {
                    var element = XElement.Parse(fccOptions);
                    fccOptionsElements.Add(element);
                    ascend = !this.IsTopLevel(element);
                }
                catch
                {

                }
            }

            return ascend;
        }

        private bool IsTopLevel(XElement root)
        {
            bool topLevel = false;
            XAttribute topLevelAttribute = root.Attribute(topLevelAttributeName);
            if (topLevelAttribute?.Value.ToLower() == "true")
            {
                topLevel = true;
            }

            return topLevel;
        }

        private string GetFCCOptionsPath(string directory) => Path.Combine(directory, fccOptionsFileName);
    }
}
