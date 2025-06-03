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
        private ThemeResourceKey gridLinesBrushKey;
        private ThemeResourceKey backgroundBrushKey;
        private ThemeResourceKey backgroundIsMouseOverBrushKey;
        private ThemeResourceKey backgroundIsPressedBrushKey;
        private ThemeResourceKey foregroundBrushKey;
        private ThemeResourceKey foregroundIsMouseOverBrushKey;
        private ThemeResourceKey foregroundIsPressedBrushKey;
        private bool headerUseTabularSharedColors;

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

            this.headerUseTabularSharedColors = reportOptionsProvider.Get().HeaderUseTabularSharedColors;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                bool newHeaderUseTabularSharedColors = newOptions.HeaderUseTabularSharedColors;
                if (this.headerUseTabularSharedColors != newHeaderUseTabularSharedColors)
                {
                    this.headerUseTabularSharedColors = newHeaderUseTabularSharedColors;
                    this.SetHeaderForegroundColors();
                    this.OnPropertyChanged(nameof(this.ForegroundBrushKey));
                    this.OnPropertyChanged(nameof(this.ForegroundIsMouseOverBrushKey));
                    this.OnPropertyChanged(nameof(this.ForegroundIsPressedBrushKey));
                }
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
            if (this.headerUseTabularSharedColors)
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
            get => this.gridLinesBrushKey;
            set => this.Set(ref this.gridLinesBrushKey, value);
        }

        public ThemeResourceKey BackgroundBrushKey
        {
            get => this.backgroundBrushKey;
            set => this.Set(ref this.backgroundBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsMouseOverBrushKey
        {
            get => this.backgroundIsMouseOverBrushKey;
            set => this.Set(ref this.backgroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsPressedBrushKey
        {
            get => this.backgroundIsPressedBrushKey;
            set => this.Set(ref this.backgroundIsPressedBrushKey, value);
        }

        public ThemeResourceKey ForegroundBrushKey
        {
            get => this.foregroundBrushKey;
            set => this.Set(ref this.foregroundBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsMouseOverBrushKey
        {
            get => this.foregroundIsMouseOverBrushKey;
            set => this.Set(ref this.foregroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsPressedBrushKey
        {
            get => this.foregroundIsPressedBrushKey;
            set => this.Set(ref this.foregroundIsPressedBrushKey, value);
        }
    }
}