using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoverageProjectSettingsManager
    {
        Task<ICoverageSettings> GetSettingsAsync(ICoverageProject coverageProject);
    }
}
