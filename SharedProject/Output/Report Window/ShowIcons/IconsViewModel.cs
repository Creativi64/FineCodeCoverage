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
                if (!this.Monochrome)
                {
                    return;
                }

                this.SetIconStyles();
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

                if (newAppOptions.ThemedIconStyle == this._themedIconStyle)
                {
                    return;
                }

                this._themedIconStyle = newAppOptions.ThemedIconStyle;
                this.SetIconStyles();
            };
            ReportOptions appOptions = reportOptionsProvider.Get();
            this.ShowIcons = appOptions.ShowIcons;
            this.IconSize = appOptions.IconSize;
            this._themedIconStyle = appOptions.ThemedIconStyle;
            this.SetIconStyles();
        }

        private void SetIconStyles()
        {
            switch (this._themedIconStyle)
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
            get => this._showIcons;
            private set => this.Set(ref this._showIcons, value);
        }

        public int IconSize
        {
            get => this._iconSize;
            private set => this.Set(ref this._iconSize, value);
        }

        public bool Monochrome
        {
            get => this._monochrome;
            private set => this.Set(ref this._monochrome, value);
        }

        public Color MonochromeColor
        {
            get => this._monochromeColor;
            private set => this.Set(ref this._monochromeColor, value);
        }
    }
}
