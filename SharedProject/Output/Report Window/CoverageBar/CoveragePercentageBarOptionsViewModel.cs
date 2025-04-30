using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Options;
using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal class InvertPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1 - (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [Export(typeof(CoveragePercentageBarOptionsViewModel))]
    internal class CoveragePercentageBarOptionsViewModel : ObservableBase, IListener<CoverageColoursChangedMessage>
    {
        class FontsAndColorsCoverageBarColours {
            public FontsAndColorsCoverageBarColours(Color coveredColor, Color notCoveredColor)
            {
                CoveredColor = coveredColor;
                NotCoveredColor = notCoveredColor;
            }

            public Color CoveredColor { get; }
            public Color NotCoveredColor { get; }
        }

        private CoveragePercentageBarStyle coveragePercentageBarStyle;
        private bool coveredPercentageLeft;
        private bool themeCoveragePercentageBar;
        private bool useSolidBrush;
        private Color coveredColor;
        private Color notCoveredColor;
        private readonly ICoverageColoursProvider coverageColoursProvider;
        private FontsAndColorsCoverageBarColours fontsAndColorsCoverageBarColours;
        private bool coverageColoursDirty = true;
        private bool lastCoveragePercentageBarColorsFromFontsAndColors;
        [ImportingConstructor]
        public CoveragePercentageBarOptionsViewModel(
            IAppOptionsProvider appOptionsProvider,
            IEventAggregator eventAggregator,
            ICoverageColoursProvider coverageColoursProvider
        )
        {
            this.coverageColoursProvider = coverageColoursProvider;
            eventAggregator.AddListener(this);
            appOptionsProvider.OptionsChanged += this.SetOptions;
            var appOptions = appOptionsProvider.Get();
            lastCoveragePercentageBarColorsFromFontsAndColors = !appOptions.CoveragePercentageBarColorsFromFontsAndColors;
            SetOptions(appOptions);
        }

        private void SetOptions(IAppOptions appOptions)
        {
            this.CoveragePercentageBarStyle = appOptions.CoveragePercentageBarStyle;
            this.CoveredPercentageLeft = appOptions.CoveredPercentageLeft;
            this.ThemeCoveragePercentageBar = appOptions.ThemeCoveragePercentageBar;
            this.UseSolidBrush = appOptions.CoveragePercentageSolidBrush;
            if (appOptions.CoveragePercentageBarColorsFromFontsAndColors != lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                SetCoverageColours(appOptions.CoveragePercentageBarColorsFromFontsAndColors);
                lastCoveragePercentageBarColorsFromFontsAndColors = appOptions.CoveragePercentageBarColorsFromFontsAndColors;
            }
        }

        private void SetCoverageColours(bool coveragePercentageBarColorsFromFontsAndColors)
        {
            if (coveragePercentageBarColorsFromFontsAndColors)
            {
                SetCoverageColoursFromFontsAndColors();
            }
            else
            {
                NotCoveredColor = VisualStudioNotificationColors.Red;
                CoveredColor = VisualStudioNotificationColors.Green;
            }
        }

        private void SetCoverageColoursFromFontsAndColors()
        {
            var fontsAndColorsCoverageBarColours = GetFontsAndColorsCoverageColours();
            NotCoveredColor = fontsAndColorsCoverageBarColours.NotCoveredColor;
            CoveredColor = fontsAndColorsCoverageBarColours.CoveredColor;
        }

        private FontsAndColorsCoverageBarColours GetFontsAndColorsCoverageColours()
        {
            if (fontsAndColorsCoverageBarColours == null || coverageColoursDirty)
            {
                var coverageColours = coverageColoursProvider.GetCoverageColours();
                var coveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.Covered).Background;
                var notCoveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.NotCovered).Background;
                fontsAndColorsCoverageBarColours = new FontsAndColorsCoverageBarColours(coveredColor, notCoveredColor);
                coverageColoursDirty = false;
            }
            return fontsAndColorsCoverageBarColours;
        }

        public void Handle(CoverageColoursChangedMessage message)
        {
            coverageColoursDirty = true;
            if (lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                SetCoverageColoursFromFontsAndColors();
            }
        }

        public CoveragePercentageBarStyle CoveragePercentageBarStyle
        {
            get => this.coveragePercentageBarStyle;
            set => this.Set(ref coveragePercentageBarStyle, value);
        }

        public bool CoveredPercentageLeft
        {
            get => this.coveredPercentageLeft;
            set => this.Set(ref coveredPercentageLeft, value);
        }

        public bool ThemeCoveragePercentageBar
        {
            get => this.themeCoveragePercentageBar;
            set => this.Set(ref themeCoveragePercentageBar, value);
        }

        public Color CoveredColor
        {
            get => this.coveredColor;
            set => this.Set(ref coveredColor, value);
        }

        public Color NotCoveredColor
        {
            get => this.notCoveredColor;
            set => this.Set(ref notCoveredColor, value);
        }

        public bool UseSolidBrush
        {
            get => this.useSolidBrush;
            set => this.Set(ref useSolidBrush, value);
        }
    }
}
