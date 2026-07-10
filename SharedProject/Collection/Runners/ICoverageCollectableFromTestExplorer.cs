using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.Runners
{
    internal interface ICoverageCollectableFromTestExplorer
    {
        Task<bool> IsCollectableAsync();
    }
}
