using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ISettingsMerger
    {
        Task MergeAsync(
            CoverageSettings coverageSettings,
            List<System.Reflection.PropertyInfo> coverageSettingsPropertyInfos,
            List<XElement> settingsFileElements,
            XElement projectSettingsElement
        );
    }
}