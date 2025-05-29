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
        )
        {
            this.fileUtil = fileUtil;
        }

        public List<XElement> Provide(string projectDirectoryPath)
        {
            List<XElement> fccOptionsElements = new List<XElement>();
            string directoryPath = projectDirectoryPath;
            bool ascend = true;
            while (ascend)
            {
                ascend = AddFromDirectory(fccOptionsElements, directoryPath);
                if (ascend)
                {
                    directoryPath = fileUtil.DirectoryParentPath(directoryPath);
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
            string fccOptionsPath = GetFCCOptionsPath(directory);
            if (fileUtil.Exists(fccOptionsPath))
            {
                string fccOptions = fileUtil.ReadAllText(fccOptionsPath);
                try
                {
                    XElement element = XElement.Parse(fccOptions);
                    fccOptionsElements.Add(element);
                    ascend = !IsTopLevel(element);
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

        private string GetFCCOptionsPath(string directory)
        {
            return Path.Combine(directory, fccOptionsFileName);
        }

    }

}
