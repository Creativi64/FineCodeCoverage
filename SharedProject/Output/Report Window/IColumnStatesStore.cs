using System.Threading.Tasks;

namespace FineCodeCoverage.Output
{
    interface IColumnStatesStore
    {
        Task SaveColumnStatesAsync(string columnStates);
        Task<string> GetColumnStatesAsync();
    }
}