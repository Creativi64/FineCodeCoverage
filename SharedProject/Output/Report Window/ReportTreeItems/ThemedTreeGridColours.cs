using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    internal sealed class ThemedTreeGridColours
    {
        private ThemedTreeGridColours()
        {
            VSColorTheme.ThemeChanged += (_) => this.PopulateColors();
            PopulateColors();
        }

        public static ThemedTreeGridColours Instance { get; } = new ThemedTreeGridColours();

        public Brush TransparentBrush { get; } = new SolidColorBrush(Colors.Transparent);

        public Brush SelectedItemActiveBackColor { get; internal set; }

        public Brush SelectedItemActiveForeColor { get; internal set; }

        public Brush SelectedItemInactiveBackColor { get; internal set; }

        public Brush SelectedItemInactiveForeColor { get; internal set; }

        public Brush ForegroundColor { get; internal set; }

        private void PopulateColors()
        {
            this.SelectedItemActiveBackColor = TreeViewColors.SelectedItemActiveColorKey.ToBrush();
            this.SelectedItemActiveForeColor = TreeViewColors.SelectedItemActiveTextColorKey.ToBrush();
            this.SelectedItemInactiveBackColor = TreeViewColors.SelectedItemInactiveColorKey.ToBrush();
            this.SelectedItemInactiveForeColor = TreeViewColors.SelectedItemInactiveTextColorKey.ToBrush();
            this.ForegroundColor = TreeViewColors.BackgroundTextColorKey.ToBrush();
        }
   }
}

