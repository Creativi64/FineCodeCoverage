using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.PlatformUI;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IIconMeasurementOptions))]
    internal sealed class IconsViewModel : ObservableBase, IIconMeasurementOptions
    {
        private bool _showIcons;
        private int _iconSize;
        private ThemedIconStyle _themedIconStyle;
        private bool _monochrome;
        private Color _monochromeColor;

        public event EventHandler ShowIconsChanged;

        public event EventHandler IconSizeChanged;

        [ImportingConstructor]
        public IconsViewModel(IOptionsProvider<ReportOptions> reportOptionsProvider)
        {
            VSColorTheme.ThemeChanged += (_) =>
            {
                if (!Monochrome)
                {
                    return;
                }

                SetIconStyles();
            };
            reportOptionsProvider.OptionsChanged += (newAppOptions) =>
            {
                if (newAppOptions.ShowIcons != ShowIcons)
                {
                    ShowIcons = newAppOptions.ShowIcons;
                    ShowIconsChanged?.Invoke(this, EventArgs.Empty);
                }

                if (newAppOptions.IconSize != IconSize)
                {
                    IconSize = newAppOptions.IconSize;
                    IconSizeChanged?.Invoke(this, EventArgs.Empty);
                }

                if (newAppOptions.ThemedIconStyle == _themedIconStyle)
                {
                    return;
                }

                _themedIconStyle = newAppOptions.ThemedIconStyle;
                SetIconStyles();
            };
            ReportOptions appOptions = reportOptionsProvider.Get();
            ShowIcons = appOptions.ShowIcons;
            IconSize = appOptions.IconSize;
            _themedIconStyle = appOptions.ThemedIconStyle;
            SetIconStyles();
        }

        private void SetIconStyles()
        {
            switch (_themedIconStyle)
            {
                case ThemedIconStyle.MonochromeGlyph:
                    Monochrome = true;
                    MonochromeColor = VSColorTheme.GetThemedColor(TreeViewColors.GlyphColorKey).ToMediaColor();
                    break;
                case ThemedIconStyle.MonochromeText:
                    Monochrome = true;
                    MonochromeColor = VSColorTheme.GetThemedColor(TreeViewColors.BackgroundTextColorKey).ToMediaColor();
                    break;
                case ThemedIconStyle.Moniker:
                    Monochrome = false;
                    MonochromeColor = Colors.Transparent;
                    break;
            }
        }

        public bool ShowIcons
        {
            get => _showIcons;
            private set => Set(ref _showIcons, value);
        }

        public int IconSize
        {
            get => _iconSize;
            private set => Set(ref _iconSize, value);
        }

        public bool Monochrome
        {
            get => _monochrome;
            private set => Set(ref _monochrome, value);
        }

        public Color MonochromeColor
        {
            get => _monochromeColor;
            private set => Set(ref _monochromeColor, value);
        }
    }
}
