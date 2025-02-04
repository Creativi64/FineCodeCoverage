namespace FineCodeCoverage.Output
{
    interface IColumnStatesStore
    {
        void SaveColumnStates(string columnStates);
        string GetColumnStates();
    }
}
