using System.Threading.Tasks;

namespace FineCodeCoverage.Output
{
    internal interface IColumnStatesStore
    {
        Task SaveColumnStatesAsync(string columnStates);

        Task<string> GetColumnStatesAsync();
    }
}
