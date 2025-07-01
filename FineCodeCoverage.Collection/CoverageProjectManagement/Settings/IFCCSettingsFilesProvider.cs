using System.Collections.Generic;
using System.Xml.Linq;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal interface IFCCSettingsFilesProvider
    {
        List<XElement> Provide(string projectDirectoryPath);
    }
}
