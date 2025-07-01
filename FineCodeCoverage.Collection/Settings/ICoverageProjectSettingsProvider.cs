using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoverageProjectSettingsProvider
    {
        Task<XElement> ProvideAsync(ICoverageProject coverageProject);
    }
}
