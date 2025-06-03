using System.ComponentModel.Composition;
using System.Windows.Media;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.Options;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(CoveragePercentageBarOptionsViewModel))]
    internal class CoveragePercentageBarOptionsViewModel : ObservableBase, IListener<CoverageColoursChangedMessage>
    {
        class FontsAndColorsCoverageBarColours
        {
            public FontsAndColorsCoverageBarColours(Color coveredColor, Color notCoveredColor)
            {
                this.CoveredColor = coveredColor;
                this.NotCoveredColor = notCoveredColor;
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
            IOptionsProvider<ReportOptions> reportOptionsProvider,
            IEventAggregator eventAggregator,
            ICoverageColoursProvider coverageColoursProvider
        )
        {
            this.coverageColoursProvider = coverageColoursProvider;
            _ = eventAggregator.AddListener(this);
            reportOptionsProvider.OptionsChanged += this.SetOptions;
            ReportOptions reportOptions = reportOptionsProvider.Get();
            this.lastCoveragePercentageBarColorsFromFontsAndColors = !reportOptions.CoveragePercentageUseColorsFromFontsAndColors;
            this.SetOptions(reportOptions);
        }

        private void SetOptions(ReportOptions reportOptions)
        {
            this.DisplayParts = reportOptions.CoveragePercentageDisplayParts;
            this.CoveredPercentageIsLeft = reportOptions.CoveragePercentageCoveredIsLeft;
            this.IsThemed = reportOptions.CoveragePercentageIsThemed;
            this.UseSolidBrush = reportOptions.CoveragePercentageUseSolidBrush;
            this.UseContrastedThemeWhenSingularDisplay = reportOptions.CoveragePercentageUseContrastedThemeWhenSingularDisplay;
            this.HeightOrMultiplier = reportOptions.CoveragePercentageHeightOrMultiplier;
            this.ShowToolTip = reportOptions.CoveragePercentageShowTooltip;
            if (reportOptions.CoveragePercentageUseColorsFromFontsAndColors != this.lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                this.SetCoverageColours(reportOptions.CoveragePercentageUseColorsFromFontsAndColors);
                this.lastCoveragePercentageBarColorsFromFontsAndColors = reportOptions.CoveragePercentageUseColorsFromFontsAndColors;
            }
        }

        private void SetCoverageColours(bool coveragePercentageBarColorsFromFontsAndColors)
        {
            if (coveragePercentageBarColorsFromFontsAndColors)
            {
                this.SetCoverageColoursFromFontsAndColors();
            }
            else
            {
                this.NotCoveredColor = VisualStudioNotificationColors.Negative;
                this.CoveredColor = VisualStudioNotificationColors.Positive;
            }
        }

        private void SetCoverageColoursFromFontsAndColors()
        {
            FontsAndColorsCoverageBarColours fontsAndColorsCoverageBarColours = this.GetFontsAndColorsCoverageColours();
            this.NotCoveredColor = fontsAndColorsCoverageBarColours.NotCoveredColor;
            this.CoveredColor = fontsAndColorsCoverageBarColours.CoveredColor;
        }

        private FontsAndColorsCoverageBarColours GetFontsAndColorsCoverageColours()
        {
            if (this.fontsAndColorsCoverageBarColours == null || this.coverageColoursDirty)
            {
                ICoverageColours coverageColours = this.coverageColoursProvider.GetCoverageColours();
                Color coveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.Covered).Background;
                Color notCoveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.NotCovered).Background;
                this.fontsAndColorsCoverageBarColours = new FontsAndColorsCoverageBarColours(coveredColor, notCoveredColor);
                this.coverageColoursDirty = false;
            }

            return this.fontsAndColorsCoverageBarColours;
        }

        public void Handle(CoverageColoursChangedMessage message)
        {
            this.coverageColoursDirty = true;
            if (this.lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                this.SetCoverageColoursFromFontsAndColors();
            }
        }

        public CoveragePercentageBarDisplayParts DisplayParts
        {
            get => this.displayParts;
            set => this.Set(ref this.displayParts, value);
        }

        public bool CoveredPercentageIsLeft
        {
            get => this.coveredPercentageIsLeft;
            set => this.Set(ref this.coveredPercentageIsLeft, value);
        }

        public bool IsThemed
        {
            get => this.isThemed;
            set => this.Set(ref this.isThemed, value);
        }

        public Color CoveredColor
        {
            get => this.coveredColor;
            set => this.Set(ref this.coveredColor, value);
        }

        public Color NotCoveredColor
        {
            get => this.notCoveredColor;
            set => this.Set(ref this.notCoveredColor, value);
        }

        public bool UseSolidBrush
        {
            get => this.useSolidBrush;
            set => this.Set(ref this.useSolidBrush, value);
        }

        public bool UseContrastedThemeWhenSingularDisplay
        {
            get => this.useContrastThemeWhenSingularDisplay;
            set => this.Set(ref this.useContrastThemeWhenSingularDisplay, value);
        }

        public double? HeightOrMultiplier
        {
            get => this.heightOrMultiplier;
            set => this.Set(ref this.heightOrMultiplier, value);
        }

        public bool ShowToolTip
        {
            get => this.showToolTip;
            set => this.Set(ref this.showToolTip, value);
        }
    }
}