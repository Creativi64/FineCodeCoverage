using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using WpfHelpers;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;

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
            VSColorTheme.ThemeChanged += VsColorTheme_ThemeChanged;
            GridLinesBrushKey = HeaderColors.SeparatorLineBrushKey;
            BackgroundBrushKey = HeaderColors.DefaultBrushKey;
            BackgroundIsMouseOverBrushKey = HeaderColors.MouseOverBrushKey;
            BackgroundIsPressedBrushKey = HeaderColors.MouseDownBrushKey;

            headerUseTabularSharedColors = reportOptionsProvider.Get().HeaderUseTabularSharedColors;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                var newHeaderUseTabularSharedColors = newOptions.HeaderUseTabularSharedColors;
                if (headerUseTabularSharedColors != newHeaderUseTabularSharedColors)
                {
                    headerUseTabularSharedColors = newHeaderUseTabularSharedColors;
                    SetHeaderForegroundColors();
                    OnPropertyChanged(nameof(ForegroundBrushKey));
                    OnPropertyChanged(nameof(ForegroundIsMouseOverBrushKey));
                    OnPropertyChanged(nameof(ForegroundIsPressedBrushKey));
                }
            };
            SetHeaderForegroundColors();
        }

        private void VsColorTheme_ThemeChanged(EventArgs _)
        {
            OnPropertyChanged(nameof(GridLinesBrushKey));
            OnPropertyChanged(nameof(BackgroundBrushKey));
            OnPropertyChanged(nameof(BackgroundIsMouseOverBrushKey));
            OnPropertyChanged(nameof(BackgroundIsPressedBrushKey));
            OnPropertyChanged(nameof(ForegroundBrushKey));
            OnPropertyChanged(nameof(ForegroundIsMouseOverBrushKey));
            OnPropertyChanged(nameof(ForegroundIsPressedBrushKey));
        }

        private void SetHeaderForegroundColors()
        {
            if (headerUseTabularSharedColors)
            {
                ForegroundBrushKey = EnvironmentColors.CommandBarTextActiveBrushKey;
                ForegroundIsMouseOverBrushKey = EnvironmentColors.CommandBarTextHoverBrushKey;
                ForegroundIsPressedBrushKey = EnvironmentColors.CommandBarTextSelectedBrushKey;
            }
            else
            {
                ForegroundBrushKey = HeaderColors.DefaultTextBrushKey;
                ForegroundIsMouseOverBrushKey = HeaderColors.MouseOverTextBrushKey;
                ForegroundIsPressedBrushKey = HeaderColors.MouseDownTextBrushKey;
            }
        }

        public ThemeResourceKey GridLinesBrushKey
        {
            get => this.gridLinesBrushKey;
            set => this.Set(ref gridLinesBrushKey, value);
        }

        public ThemeResourceKey BackgroundBrushKey
        {
            get => this.backgroundBrushKey;
            set => this.Set(ref backgroundBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsMouseOverBrushKey
        {
            get => this.backgroundIsMouseOverBrushKey;
            set => this.Set(ref backgroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsPressedBrushKey
        {
            get => this.backgroundIsPressedBrushKey;
            set => this.Set(ref backgroundIsPressedBrushKey, value);
        }

        public ThemeResourceKey ForegroundBrushKey
        {
            get => this.foregroundBrushKey;
            set => this.Set(ref foregroundBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsMouseOverBrushKey
        {
            get => this.foregroundIsMouseOverBrushKey;
            set => this.Set(ref foregroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsPressedBrushKey
        {
            get => this.foregroundIsPressedBrushKey;
            set => this.Set(ref foregroundIsPressedBrushKey, value);
        }
    }
}
