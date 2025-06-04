using System.ComponentModel;
using System.Windows.Media;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal sealed class ThemedTreeGridColours : INotifyPropertyChanged
    {
        private ThemedTreeGridColours()
        {
            VSColorTheme.ThemeChanged += (_) => this.PopulateColors();
            this.PopulateColors();
        }

        public static ThemedTreeGridColours Instance { get; } = new ThemedTreeGridColours();

        private readonly ThemeResourceKey _imageBackgroundThemeResourceKey = TreeViewColors.BackgroundColorKey;
        private readonly ThemeResourceKey _imageBackgroundFallbackThemeResourceKey = EnvironmentColors.ToolWindowBackgroundColorKey;

        public event PropertyChangedEventHandler PropertyChanged;

        public Color ImageBackgroundColor { get; private set; }
        public Brush ImageBackgroundBrush { get; private set; }

        public Brush TransparentBrush { get; } = new SolidColorBrush(Colors.Transparent);

        public Brush SelectedItemActiveBackColor { get; private set; }

        public Brush SelectedItemActiveForeColor { get; private set; }

        public Brush SelectedItemInactiveBackColor { get; private set; }

        public Brush SelectedItemInactiveForeColor { get; internal set; }

        public Brush ForegroundColor { get; internal set; }

        private void SetImageBackgroundColor()
        {
            this.ImageBackgroundColor = this._imageBackgroundThemeResourceKey.ToColor();
            if (this.ImageBackgroundColor == Colors.Transparent)
            {
                this.ImageBackgroundColor = this._imageBackgroundFallbackThemeResourceKey.ToColor();
            }

            this.ImageBackgroundBrush = new SolidColorBrush(this.ImageBackgroundColor);
            this.ImageBackgroundBrush.Freeze();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ImageBackgroundColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ImageBackgroundBrush)));
        }

        private void PopulateColors()
        {
            this.SetImageBackgroundColor();
            this.SelectedItemActiveBackColor = TreeViewColors.SelectedItemActiveColorKey.ToBrush();
            this.SelectedItemActiveForeColor = TreeViewColors.SelectedItemActiveTextColorKey.ToBrush();
            this.SelectedItemInactiveBackColor = TreeViewColors.SelectedItemInactiveColorKey.ToBrush();
            this.SelectedItemInactiveForeColor = TreeViewColors.SelectedItemInactiveTextColorKey.ToBrush();
            this.ForegroundColor = TreeViewColors.BackgroundTextColorKey.ToBrush();
        }
    }
}