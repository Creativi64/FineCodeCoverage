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
        internal const string FCCOptionsFileName = "finecodecoverage-settings.xml";
        private const string TopLevelAttributeName = "topLevel";
        private readonly IFileUtil _fileUtil;

        [ImportingConstructor]
        public FCCSettingsFilesProvider(
            IFileUtil fileUtil
        ) => this._fileUtil = fileUtil;

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
                    directoryPath = this._fileUtil.DirectoryParentPath(directoryPath);
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
            if (this._fileUtil.Exists(fccOptionsPath))
            {
                string fccOptions = this._fileUtil.ReadAllText(fccOptionsPath);
                try
                {
                    var element = XElement.Parse(fccOptions);
                    fccOptionsElements.Add(element);
                    ascend = !IsTopLevel(element);
                }
                catch
                {

                }
            }

            return ascend;
        }

        private static bool IsTopLevel(XElement root)
        {
            bool topLevel = false;
            XAttribute topLevelAttribute = root.Attribute(TopLevelAttributeName);
            if (topLevelAttribute?.Value.ToLower() == "true")
            {
                topLevel = true;
            }

            return topLevel;
        }

        private static string GetFCCOptionsPath(string directory) => Path.Combine(directory, FCCOptionsFileName);
    }
}