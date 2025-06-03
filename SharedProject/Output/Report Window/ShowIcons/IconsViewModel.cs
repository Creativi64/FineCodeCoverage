using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.PlatformUI;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IIconsOptions))]
    internal class IconsViewModel : ObservableBase, IIconsOptions
    {
        private bool showIcons;
        private int iconSize;
        public event EventHandler ShowIconsChanged;
        public event EventHandler IconSizeChanged;
        private ThemedIconStyle themedIconStyle;

        [ImportingConstructor]
        public IconsViewModel(IOptionsProvider<ReportOptions> reportOptionsProvider)
        {
            VSColorTheme.ThemeChanged += (_) =>
            {
                if (this.Monochrome)
                {
                    this.SetIconStyles();
                }
            };
            reportOptionsProvider.OptionsChanged += (newAppOptions) =>
            {
                if (newAppOptions.ShowIcons != this.ShowIcons)
                {
                    this.ShowIcons = newAppOptions.ShowIcons;
                    ShowIconsChanged?.Invoke(this, EventArgs.Empty);
                }

                if (newAppOptions.IconSize != this.IconSize)
                {
                    this.IconSize = newAppOptions.IconSize;
                    IconSizeChanged?.Invoke(this, EventArgs.Empty);
                }

                if (newAppOptions.ThemedIconStyle != this.themedIconStyle)
                {
                    this.themedIconStyle = newAppOptions.ThemedIconStyle;
                    this.SetIconStyles();
                }
            };
            ReportOptions appOptions = reportOptionsProvider.Get();
            this.ShowIcons = appOptions.ShowIcons;
            this.IconSize = appOptions.IconSize;
            this.themedIconStyle = appOptions.ThemedIconStyle;
            this.SetIconStyles();
        }

        private void SetIconStyles()
        {
            switch (this.themedIconStyle)
            {
                case ThemedIconStyle.MonochromeGlyph:
                    this.Monochrome = true;
                    this.MonochromeColor = VSColorTheme.GetThemedColor(TreeViewColors.GlyphColorKey).ToMediaColor();
                    break;
                case ThemedIconStyle.MonochromeText:
                    this.Monochrome = true;
                    this.MonochromeColor = VSColorTheme.GetThemedColor(TreeViewColors.BackgroundTextColorKey).ToMediaColor();
                    break;
                case ThemedIconStyle.Moniker:
                    this.Monochrome = false;
                    this.MonochromeColor = Colors.Transparent;
                    break;
            }
        }

        public bool ShowIcons
        {
            get => this.showIcons;
            private set => this.Set(ref this.showIcons, value);
        }

        public int IconSize
        {
            get => this.iconSize;
            private set => this.Set(ref this.iconSize, value);
        }

        private bool monochrome;
        public bool Monochrome
        {
            get => this.monochrome;
            private set => this.Set(ref this.monochrome, value);
        }

        private Color monochromeColor;
        public Color MonochromeColor
        {
            get => this.monochromeColor;
            private set => this.Set(ref this.monochromeColor, value);
        }
    }
}