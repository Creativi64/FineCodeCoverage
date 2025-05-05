using FineCodeCoverage.Options;
using System;
using System.ComponentModel.Composition;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IShowIcons))]
    internal class ShowIconsViewModel : ObservableBase, IShowIcons
    {
        private bool showIcons;
        public event EventHandler ShowIconsChanged;

        [ImportingConstructor]
        public ShowIconsViewModel(IAppOptionsProvider appOptionsProvider)
        {
            appOptionsProvider.OptionsChanged += (newAppOptions) =>
            {
                if (newAppOptions.ShowIcons != this.ShowIcons)
                {
                    this.ShowIcons = newAppOptions.ShowIcons;
                    ShowIconsChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            var appOptions = appOptionsProvider.Get();
            this.ShowIcons = appOptions.ShowIcons;
        }

        public bool ShowIcons
        {
            get => this.showIcons;
            private set => this.Set(ref this.showIcons, value);
        }
    }
}
