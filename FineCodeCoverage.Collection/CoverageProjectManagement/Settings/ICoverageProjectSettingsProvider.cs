using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal interface ICoverageProjectSettingsProvider
    {
        Task<XElement> ProvideAsync(ICoverageProject coverageProject);
    }
}
