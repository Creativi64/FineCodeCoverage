using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    internal sealed class ThemedTreeGridColours : INotifyPropertyChanged
    {
        private ThemedTreeGridColours()
        {
            VSColorTheme.ThemeChanged += (_) => this.PopulateColors();
            PopulateColors();
        }

        public static ThemedTreeGridColours Instance { get; } = new ThemedTreeGridColours();

        private ThemeResourceKey ImageBackgroundThemeResourceKey = TreeViewColors.BackgroundColorKey;
        private ThemeResourceKey ImageBackgroundFallbackThemeResourceKey = EnvironmentColors.ToolWindowBackgroundColorKey;

        public event PropertyChangedEventHandler PropertyChanged;

        public Color ImageBackgroundColor { get; private set; }

        public Brush TransparentBrush { get; } = new SolidColorBrush(Colors.Transparent);

        public Brush SelectedItemActiveBackColor { get; private set; }

        public Brush SelectedItemActiveForeColor { get; private set; }

        public Brush SelectedItemInactiveBackColor { get; private set; }

        public Brush SelectedItemInactiveForeColor { get; internal set; }

        public Brush ForegroundColor { get; internal set; }

        private void SetImageBackgroundColor()
        {
            this.ImageBackgroundColor = ImageBackgroundThemeResourceKey.ToColor();
            if(this.ImageBackgroundColor == Colors.Transparent)
            {
                this.ImageBackgroundColor = ImageBackgroundFallbackThemeResourceKey.ToColor();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageBackgroundColor)));
        }

        private void PopulateColors()
        {
            SetImageBackgroundColor();
            this.SelectedItemActiveBackColor = TreeViewColors.SelectedItemActiveColorKey.ToBrush();
            this.SelectedItemActiveForeColor = TreeViewColors.SelectedItemActiveTextColorKey.ToBrush();
            this.SelectedItemInactiveBackColor = TreeViewColors.SelectedItemInactiveColorKey.ToBrush();
            this.SelectedItemInactiveForeColor = TreeViewColors.SelectedItemInactiveTextColorKey.ToBrush();
            this.ForegroundColor = TreeViewColors.BackgroundTextColorKey.ToBrush();
        }
   }
}

