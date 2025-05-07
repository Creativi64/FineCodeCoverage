using FineCodeCoverage.Options;
using System;
using System.ComponentModel.Composition;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IIconsOptions))]
    internal class IconsViewModel : ObservableBase, IIconsOptions
    {
        private bool showIcons;
        private int iconSize;
        private bool themedMonochromeIcons;
        public event EventHandler ShowIconsChanged;
        public event EventHandler IconSizeChanged;
        public event EventHandler ThemedMonochromeIconsChanged;

        [ImportingConstructor]
        public IconsViewModel(IAppOptionsProvider appOptionsProvider)
        {
            appOptionsProvider.OptionsChanged += (newAppOptions) =>
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
                if (newAppOptions.ThemedMonochromeIcons != this.ThemedMonochromeIcons)
                {
                    this.ThemedMonochromeIcons = newAppOptions.ThemedMonochromeIcons;
                    ThemedMonochromeIconsChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            var appOptions = appOptionsProvider.Get();
            this.ShowIcons = appOptions.ShowIcons;
            this.IconSize = appOptions.IconSize;
            this.ThemedMonochromeIcons = appOptions.ThemedMonochromeIcons;
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

        public bool ThemedMonochromeIcons {
            get => this.themedMonochromeIcons;
            set => this.Set(ref this.themedMonochromeIcons, value);
        }
    }
}
