using System.Collections.Generic;

namespace FineCodeCoverage.Wpf
{
    public interface ISelectionHandler<T>
    {
        void SelectionChanged(List<T> selectedItems);
    }
}