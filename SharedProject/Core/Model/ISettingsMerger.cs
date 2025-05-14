using FineCodeCoverage.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ISettingsMerger
    {
        Task<AppOptions> MergeAsync(
            AppOptions globalOptions,
            List<XElement> settingsFileElements,
            XElement projectSettingsElement
        );
    }
}
