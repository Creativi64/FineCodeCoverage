using System.ComponentModel;
using System.Windows.Media;
using FineCodeCoverage.Utilities.Extensions;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal sealed class ThemedTreeGridColours : INotifyPropertyChanged
    {
        private ThemedTreeGridColours()
        {
            VSColorTheme.ThemeChanged += (_) => PopulateColors();
            PopulateColors();
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
            ImageBackgroundColor = _imageBackgroundThemeResourceKey.ToColor();
            if (ImageBackgroundColor == Colors.Transparent)
            {
                ImageBackgroundColor = _imageBackgroundFallbackThemeResourceKey.ToColor();
            }

            ImageBackgroundBrush = new SolidColorBrush(ImageBackgroundColor);
            ImageBackgroundBrush.Freeze();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageBackgroundColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageBackgroundBrush)));
        }

        private void PopulateColors()
        {
            SetImageBackgroundColor();
            SelectedItemActiveBackColor = TreeViewColors.SelectedItemActiveColorKey.ToBrush();
            SelectedItemActiveForeColor = TreeViewColors.SelectedItemActiveTextColorKey.ToBrush();
            SelectedItemInactiveBackColor = TreeViewColors.SelectedItemInactiveColorKey.ToBrush();
            SelectedItemInactiveForeColor = TreeViewColors.SelectedItemInactiveTextColorKey.ToBrush();
            ForegroundColor = TreeViewColors.BackgroundTextColorKey.ToBrush();
        }
    }
}
