using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    public interface IVsBuildFCCSettingsProvider
    {
        Task<XElement> GetSettingsAsync(Guid projectId);
    }
}
