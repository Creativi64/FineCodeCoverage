using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.Model
{
    public interface ICoverageProjectSettingsManager
    {
        Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject);
        ICoverageSettings GetSettings(ICoverageProject coverageProject);
    }
}
