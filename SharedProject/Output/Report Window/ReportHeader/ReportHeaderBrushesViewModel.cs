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
            IOptionsProvider<ReportOptions> reportOptionsProvider)
        {
            VSColorTheme.ThemeChanged += VsColorTheme_ThemeChanged;
            GridLinesBrushKey = HeaderColors.SeparatorLineBrushKey;
            BackgroundBrushKey = HeaderColors.DefaultBrushKey;
            BackgroundIsMouseOverBrushKey = HeaderColors.MouseOverBrushKey;
            BackgroundIsPressedBrushKey = HeaderColors.MouseDownBrushKey;

            _headerUseTabularSharedColors = reportOptionsProvider.Get().HeaderUseTabularSharedColors;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                bool newHeaderUseTabularSharedColors = newOptions.HeaderUseTabularSharedColors;
                if (_headerUseTabularSharedColors == newHeaderUseTabularSharedColors)
                {
                    return;
                }

                _headerUseTabularSharedColors = newHeaderUseTabularSharedColors;
                SetHeaderForegroundColors();
                OnPropertyChanged(nameof(ForegroundBrushKey));
                OnPropertyChanged(nameof(ForegroundIsMouseOverBrushKey));
                OnPropertyChanged(nameof(ForegroundIsPressedBrushKey));
            };
            SetHeaderForegroundColors();
        }

        private void VsColorTheme_ThemeChanged(EventArgs eventArgs)
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
            if (_headerUseTabularSharedColors)
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
            get => _gridLinesBrushKey;
            set => Set(ref _gridLinesBrushKey, value);
        }

        public ThemeResourceKey BackgroundBrushKey
        {
            get => _backgroundBrushKey;
            set => Set(ref _backgroundBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsMouseOverBrushKey
        {
            get => _backgroundIsMouseOverBrushKey;
            set => Set(ref _backgroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey BackgroundIsPressedBrushKey
        {
            get => _backgroundIsPressedBrushKey;
            set => Set(ref _backgroundIsPressedBrushKey, value);
        }

        public ThemeResourceKey ForegroundBrushKey
        {
            get => _foregroundBrushKey;
            set => Set(ref _foregroundBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsMouseOverBrushKey
        {
            get => _foregroundIsMouseOverBrushKey;
            set => Set(ref _foregroundIsMouseOverBrushKey, value);
        }

        public ThemeResourceKey ForegroundIsPressedBrushKey
        {
            get => _foregroundIsPressedBrushKey;
            set => Set(ref _foregroundIsPressedBrushKey, value);
        }
    }
}
