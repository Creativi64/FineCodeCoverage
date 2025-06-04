using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ReportHeaderBrushesViewModel))]
    internal class ReportHeaderBrushesViewModel : ObservableBase
    {
        private ThemeResourceKey _gridLinesBrushKey;
        private ThemeResourceKey _backgroundBrushKey;
        private ThemeResourceKey _backgroundIsMouseOverBrushKey;
        private ThemeResourceKey _backgroundIsPressedBrushKey;
        private ThemeResourceKey _foregroundBrushKey;
        private ThemeResourceKey _foregroundIsMouseOverBrushKey;
        private ThemeResourceKey _foregroundIsPressedBrushKey;
        private bool _headerUseTabularSharedColors;

        [ImportingConstructor]
        public ReportHeaderBrushesViewModel(
            IOptionsProvider<ReportOptions> reportOptionsProvider
        )
        {
            VSColorTheme.ThemeChanged += this.VsColorTheme_ThemeChanged;
            this.GridLinesBrushKey = HeaderColors.SeparatorLineBrushKey;
            this.BackgroundBrushKey = HeaderColors.DefaultBrushKey;
            this.BackgroundIsMouseOverBrushKey = HeaderColors.MouseOverBrushKey;
            this.BackgroundIsPressedBrushKey = HeaderColors.MouseDownBrushKey;

            this._headerUseTabularSharedColors = reportOptionsProvider.Get().HeaderUseTabularSharedColors;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                bool newHeaderUseTabularSharedColors = newOptions.HeaderUseTabularSharedColors;
                if (this._headerUseTabularSharedColors == newHeaderUseTabularSharedColors)
                {
                    return;
                }

                this._headerUseTabularSharedColors = newHeaderUseTabularSharedColors;
                this.SetHeaderForegroundColors();
                this.OnPropertyChanged(nameof(this.ForegroundBrushKey));
                this.OnPropertyChanged(nameof(this.ForegroundIsMouseOverBrushKey));
                this.OnPropertyChanged(nameof(this.ForegroundIsPressedBrushKey));
            };
            this.SetHeaderForegroundColors();
        }

        private void VsColorTheme_ThemeChanged(EventArgs _)
        {
            this.OnPropertyChanged(nameof(this.GridLinesBrushKey));
            this.OnPropertyChanged(nameof(this.BackgroundBrushKey));
            this.OnPropertyChanged(nameof(this.BackgroundIsMouseOverBrushKey));
            this.OnPropertyChanged(nameof(this.BackgroundIsPressedBrushKey));
            this.OnPropertyChanged(nameof(this.ForegroundBrushKey));
            this.OnPropertyChanged(nameof(this.ForegroundIsMouseOverBrushKey));
            this.OnPropertyChanged(nameof(this.ForegroundIsPressedBrushKey));
        }

        private void SetHeaderForegroundColors()
        {
            if (this._headerUseTabularSharedColors)
            {
                this.ForegroundBrushKey = EnvironmentColors.CommandBarTextActiveBrushKey;
                this.ForegroundIsMouseOverBrushKey = EnvironmentColors.CommandBarTextHoverBrushKey;
                this.ForegroundIsPressedBrushKey = EnvironmentColors.CommandBarTextSelectedBrushKey;
            }
            else
            {
                this.ForegroundBrushKey = HeaderColors.DefaultTextBrushKey;
                this.ForegroundIsMouseOverBrushKey = HeaderColors.MouseOverTextBrushKey;
                this.ForegroundIsPressedBrushKey = HeaderColors.MouseDownTextBrushKey;
            }
        }

        public ThemeResourceKey GridLinesBrushKey
        {
            get => this._gridLinesBrushKey;
            set => this.Set(ref this._gridLinesBrushKey, value);
        }

        public ThemeResourceKey BackgroundBrushKey
        {
            get => this._backgroundBrushKey;
            set => this.Set(ref this._backgroundBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsMouseOverBrushKey
        {
            get => this._backgroundIsMouseOverBrushKey;
            set => this.Set(ref this._backgroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsPressedBrushKey
        {
            get => this._backgroundIsPressedBrushKey;
            set => this.Set(ref this._backgroundIsPressedBrushKey, value);
        }

        public ThemeResourceKey ForegroundBrushKey
        {
            get => this._foregroundBrushKey;
            set => this.Set(ref this._foregroundBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsMouseOverBrushKey
        {
            get => this._foregroundIsMouseOverBrushKey;
            set => this.Set(ref this._foregroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsPressedBrushKey
        {
            get => this._foregroundIsPressedBrushKey;
            set => this.Set(ref this._foregroundIsPressedBrushKey, value);
        }
    }
}
