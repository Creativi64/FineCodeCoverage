using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Model
{
    public interface ICoverageProjectSettingsManager
    {
        Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject);
        ICoverageSettings GetSettings(ICoverageProject coverageProject);
    }
}
