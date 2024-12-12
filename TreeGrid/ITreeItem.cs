using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace TreeGrid
{
    public interface ITreeItem : INotifyPropertyChanged
    {
        bool IsSelectionActive { get; set; }
        GridLength AdjustedWidth { get; }
        void AdjustWidth(double value);
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        ITreeItem Parent { get; set; }
        IEnumerable<ITreeItem> Children { get; }
    }
}
