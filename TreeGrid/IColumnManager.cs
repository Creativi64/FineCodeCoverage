namespace TreeGrid
{
    public interface IColumnManager
    {
        ColumnData[] Columns { get; }

        void SortColumnsArray();
    }
}
