using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    public interface ICoverageProjectSettingsManager
    {
        Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject);
        ICoverageSettings GetSettings(ICoverageProject coverageProject);
    }
}
