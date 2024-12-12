using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    internal class ThemedTreeGridColours
    {
        private ThemedTreeGridColours()
        {

            VSColorTheme.ThemeChanged += new ThemeChangedEventHandler(this.VSColorTheme_ThemeChanged);
            this.PopulateColors();
        }

        public static ThemedTreeGridColours Instance { get; } = new ThemedTreeGridColours();

        public System.Windows.Media.Brush TransparentBrush { get; } = (System.Windows.Media.Brush)new SolidColorBrush(Colors.Transparent);

        public System.Windows.Media.Brush SelectedItemActiveBackColor { get; internal set; }

        public System.Windows.Media.Brush SelectedItemActiveForeColor { get; internal set; }

        public System.Windows.Media.Brush SelectedItemInactiveBackColor { get; internal set; }

        public System.Windows.Media.Brush SelectedItemInactiveForeColor { get; internal set; }

        public System.Windows.Media.Brush ForegroundColor { get; internal set; }

        internal void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => this.PopulateColors();

        private void PopulateColors()
        {
            this.SelectedItemActiveBackColor = (System.Windows.Media.Brush)ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemActiveColorKey));
            this.SelectedItemActiveForeColor = (System.Windows.Media.Brush)ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemActiveTextColorKey));
            this.SelectedItemInactiveBackColor = (System.Windows.Media.Brush)ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemInactiveColorKey));
            this.SelectedItemInactiveForeColor = (System.Windows.Media.Brush)ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemInactiveTextColorKey));
            this.ForegroundColor = (System.Windows.Media.Brush)ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.BackgroundTextColorKey));
        }

        private static SolidColorBrush DrawingColorToMediaBrush(System.Drawing.Color color) => new SolidColorBrush(ThemedTreeGridColours.DrawingColorToMediaColor(color));

        private static System.Windows.Media.Color DrawingColorToMediaColor(System.Drawing.Color color) => System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}

