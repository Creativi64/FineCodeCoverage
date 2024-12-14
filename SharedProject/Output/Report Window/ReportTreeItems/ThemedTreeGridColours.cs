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

        public Brush TransparentBrush { get; } = new SolidColorBrush(Colors.Transparent);

        public Brush SelectedItemActiveBackColor { get; internal set; }

        public Brush SelectedItemActiveForeColor { get; internal set; }

        public Brush SelectedItemInactiveBackColor { get; internal set; }

        public Brush SelectedItemInactiveForeColor { get; internal set; }

        public Brush ForegroundColor { get; internal set; }

        internal void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => this.PopulateColors();

        private void PopulateColors()
        {
            this.SelectedItemActiveBackColor = ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemActiveColorKey));
            this.SelectedItemActiveForeColor = ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemActiveTextColorKey));
            this.SelectedItemInactiveBackColor = ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemInactiveColorKey));
            this.SelectedItemInactiveForeColor = ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemInactiveTextColorKey));
            this.ForegroundColor = ThemedTreeGridColours.DrawingColorToMediaBrush(VSColorTheme.GetThemedColor(TreeViewColors.BackgroundTextColorKey));
        }

        private static SolidColorBrush DrawingColorToMediaBrush(System.Drawing.Color color) => new SolidColorBrush(ThemedTreeGridColours.DrawingColorToMediaColor(color));

        private static Color DrawingColorToMediaColor(System.Drawing.Color color) => System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}

