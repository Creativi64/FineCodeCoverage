using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WpfHelpers;

namespace TreeGrid
{
    public abstract class TreeItemBase : ObservableBase, ITreeItem
    {
        private bool _isSelected;
        protected double _rootWidth;
        //private GridLength _adjustedWidth;
        private bool _isSelectionActive = true;
        //private double translateX;

        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                this.Set(ref this._isSelected, value, nameof(IsSelected));
                this._isSelectionActive = true;
                this.OnPropertyChanged("Background");
                this.OnPropertyChanged("Foreground");
            }
        }

        public Brush Background
        {
            get
            {
                if (!this.IsSelected)
                    return NotSelectedBackgroundBrush;
                return this.IsSelectionActive ? SelectedActiveBackgroundBrush : SelectedInactiveBackgroundBrush;
            }
        }
        private static readonly Brush transparentBrush = new SolidColorBrush(Colors.Transparent);
        protected virtual Brush NotSelectedBackgroundBrush { get => transparentBrush; }
        protected virtual Brush SelectedInactiveBackgroundBrush { get; } = SystemColors.InactiveSelectionHighlightBrush;
        protected virtual Brush SelectedActiveBackgroundBrush { get; } = SystemColors.HighlightBrush;

        protected virtual Brush NotSelectedForegroundBrush { get; } = SystemColors.ControlTextBrush;
        protected virtual Brush SelectedInactiveForegroundBrush { get; } = SystemColors.InactiveSelectionHighlightTextBrush;
        protected virtual Brush SelectedActiveForegroundBrush { get; } = SystemColors.HighlightTextBrush;


        public Brush Foreground
        {
            get
            {
                if (!this.IsSelected)
                    return NotSelectedForegroundBrush;
                return this.IsSelectionActive ? SelectedActiveForegroundBrush : SelectedInactiveForegroundBrush;
            }
        }

        public bool IsSelectionActive
        {
            get => this._isSelectionActive;
            set
            {
                this._isSelectionActive = value;
                if (!this.IsSelected)
                    return;
                this.OnPropertyChanged("Background");
                this.OnPropertyChanged("Foreground");
            }
        }
        public abstract bool IsExpanded { get; set; }
        public ITreeItem Parent { get; set; }
        public IEnumerable<ITreeItem> Children { get; protected set; }

        //public GridLength AdjustedWidth
        //{
        //    get => this._adjustedWidth;
        //    set
        //    {
        //        this.Set(ref this._adjustedWidth, value, nameof(AdjustedWidth));
        //    }
        //}
        private Adjustment adjustment;
        public Adjustment Adjustment
        {
            get => adjustment;
            set
            {
                Set(ref adjustment, value);
            }
        }
        //public double TranslateX
        //{
        //    get => translateX;
        //    set
        //    {
        //        this.Set(ref translateX, value, nameof(TranslateX));
        //    }
        //}
        private int Depth => this.Parent != null ? (this.Parent as TreeItemBase).Depth + 1 : 1;
        protected virtual double AdditionalAdjustment => 0.0;   
        public void AdjustWidth(double width)
        {
            this._rootWidth = width;
            var adjustedWidth = GetAdjustedWidth(Depth);
            var pixels = width - adjustedWidth;
            var translateX = pixels > 0 ? 0 : pixels;
            //if(pixels < 0)
            //{
            //    TranslateX = pixels;
            //}
            var clampedPixels = Math.Max(pixels, 0);
            this.Adjustment = new Adjustment(new GridLength(clampedPixels), translateX);
            //this.AdjustedWidth = new GridLength(clampedPixels);
            foreach (var treeItem in this.Children)
                treeItem.AdjustWidth(width);
        }

        protected virtual double GetAdjustedWidth(int depth) => (19 * depth) + AdditionalAdjustment;
    }
}
