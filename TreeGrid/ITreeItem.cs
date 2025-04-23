using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace TreeGrid
{
    public class Adjustment
    {
        public Adjustment(GridLength width, double translateX)
        {
            Width = width;
            TranslateX = translateX;
        }

        public GridLength Width { get; }
        public double TranslateX { get; }
    }
    public interface ITreeItem : INotifyPropertyChanged
    {
        bool IsSelectionActive { get; set; }
        Adjustment Adjustment { get; }
        //GridLength AdjustedWidth { get; }
        void AdjustWidth(double value);
       //  double TranslateX { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        ITreeItem Parent { get; set; }
        IEnumerable<ITreeItem> Children { get; }
    }
}
