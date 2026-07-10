using System.Threading.Tasks;
using FineCodeCoverage.Collection.TestExplorer.InternalTypes;

namespace FineCodeCoverage.Collection.TestExplorer
{
    internal interface IRunSettingsRetriever
    {
        Task<string> GetRunSettingsFileAsync(object userSettings, ContainerData projectData);
    }
}
