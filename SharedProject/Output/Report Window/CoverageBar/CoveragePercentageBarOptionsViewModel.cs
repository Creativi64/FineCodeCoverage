using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Options;
using System.ComponentModel.Composition;
using System.Windows.Media;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
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

        private CoveragePercentageBarDisplayParts displayParts;
        private bool coveredPercentageIsLeft;
        private bool isThemed;
        private bool useContrastThemeWhenSingularDisplay;
        private double? heightOrMultiplier;
        private bool useSolidBrush;
        private bool showToolTip;
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
            lastCoveragePercentageBarColorsFromFontsAndColors = !appOptions.CoveragePercentageUseColorsFromFontsAndColors;
            SetOptions(appOptions);
        }

        private void SetOptions(ICoveragePercentageBarOptions coveragePercentageBarOptions)
        {
            this.DisplayParts = coveragePercentageBarOptions.CoveragePercentageDisplayParts;
            this.CoveredPercentageIsLeft = coveragePercentageBarOptions.CoveragePercentageCoveredIsLeft;
            this.IsThemed = coveragePercentageBarOptions.CoveragePercentageIsThemed;
            this.UseSolidBrush = coveragePercentageBarOptions.CoveragePercentageUseSolidBrush;
            this.UseContrastedThemeWhenSingularDisplay = coveragePercentageBarOptions.CoveragePercentageUseContrastedThemeWhenSingularDisplay;
            this.HeightOrMultiplier = coveragePercentageBarOptions.CoveragePercentageHeightOrMultiplier;
            this.ShowToolTip = coveragePercentageBarOptions.CoveragePercentageShowTooltip;
            if (coveragePercentageBarOptions.CoveragePercentageUseColorsFromFontsAndColors != lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                SetCoverageColours(coveragePercentageBarOptions.CoveragePercentageUseColorsFromFontsAndColors);
                lastCoveragePercentageBarColorsFromFontsAndColors = coveragePercentageBarOptions.CoveragePercentageUseColorsFromFontsAndColors;
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
                NotCoveredColor = VisualStudioNotificationColors.Negative;
                CoveredColor = VisualStudioNotificationColors.Positive;
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

        public CoveragePercentageBarDisplayParts DisplayParts
        {
            get => this.displayParts;
            set => this.Set(ref displayParts, value);
        }

        public bool CoveredPercentageIsLeft
        {
            get => this.coveredPercentageIsLeft;
            set => this.Set(ref coveredPercentageIsLeft, value);
        }

        public bool IsThemed
        {
            get => this.isThemed;
            set => this.Set(ref isThemed, value);
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

        public bool UseContrastedThemeWhenSingularDisplay
        {
            get => this.useContrastThemeWhenSingularDisplay;
            set => this.Set(ref useContrastThemeWhenSingularDisplay, value);
        }

        public double? HeightOrMultiplier
        {
            get => this.heightOrMultiplier;
            set => this.Set(ref heightOrMultiplier, value);
        }

        public bool ShowToolTip
        {
            get => this.showToolTip;
            set => this.Set(ref showToolTip, value);
        }
    }
}
