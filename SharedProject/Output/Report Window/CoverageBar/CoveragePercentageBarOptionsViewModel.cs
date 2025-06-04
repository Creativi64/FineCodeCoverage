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

        private CoveragePercentageBarDisplayParts _displayParts;
        private bool _coveredPercentageIsLeft;
        private bool _isThemed;
        private bool _useContrastThemeWhenSingularDisplay;
        private double? _heightOrMultiplier;
        private bool _useSolidBrush;
        private bool _showToolTip;
        private Color _coveredColor;
        private Color _notCoveredColor;
        private readonly ICoverageColoursProvider _coverageColoursProvider;
        private FontsAndColorsCoverageBarColours _fontsAndColorsCoverageBarColours;
        private bool _coverageColoursDirty = true;
        private bool _lastCoveragePercentageBarColorsFromFontsAndColors;

        [ImportingConstructor]
        public CoveragePercentageBarOptionsViewModel(
            IOptionsProvider<ReportOptions> reportOptionsProvider,
            IEventAggregator eventAggregator,
            ICoverageColoursProvider coverageColoursProvider
        )
        {
            this._coverageColoursProvider = coverageColoursProvider;
            _ = eventAggregator.AddListener(this);
            reportOptionsProvider.OptionsChanged += this.SetOptions;
            ReportOptions reportOptions = reportOptionsProvider.Get();
            this._lastCoveragePercentageBarColorsFromFontsAndColors = !reportOptions.CoveragePercentageUseColorsFromFontsAndColors;
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
            if (reportOptions.CoveragePercentageUseColorsFromFontsAndColors == this._lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                return;
            }

            this.SetCoverageColours(reportOptions.CoveragePercentageUseColorsFromFontsAndColors);
            this._lastCoveragePercentageBarColorsFromFontsAndColors = reportOptions.CoveragePercentageUseColorsFromFontsAndColors;
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
            if (this._fontsAndColorsCoverageBarColours == null || this._coverageColoursDirty)
            {
                ICoverageColours coverageColours = this._coverageColoursProvider.GetCoverageColours();
                Color coveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.Covered).Background;
                Color notCoveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.NotCovered).Background;
                this._fontsAndColorsCoverageBarColours = new FontsAndColorsCoverageBarColours(coveredColor, notCoveredColor);
                this._coverageColoursDirty = false;
            }

            return this._fontsAndColorsCoverageBarColours;
        }

        public void Handle(CoverageColoursChangedMessage message)
        {
            this._coverageColoursDirty = true;
            if (!this._lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                return;
            }

            this.SetCoverageColoursFromFontsAndColors();
        }

        public CoveragePercentageBarDisplayParts DisplayParts
        {
            get => this._displayParts;
            set => this.Set(ref this._displayParts, value);
        }

        public bool CoveredPercentageIsLeft
        {
            get => this._coveredPercentageIsLeft;
            set => this.Set(ref this._coveredPercentageIsLeft, value);
        }

        public bool IsThemed
        {
            get => this._isThemed;
            set => this.Set(ref this._isThemed, value);
        }

        public Color CoveredColor
        {
            get => this._coveredColor;
            set => this.Set(ref this._coveredColor, value);
        }

        public Color NotCoveredColor
        {
            get => this._notCoveredColor;
            set => this.Set(ref this._notCoveredColor, value);
        }

        public bool UseSolidBrush
        {
            get => this._useSolidBrush;
            set => this.Set(ref this._useSolidBrush, value);
        }

        public bool UseContrastedThemeWhenSingularDisplay
        {
            get => this._useContrastThemeWhenSingularDisplay;
            set => this.Set(ref this._useContrastThemeWhenSingularDisplay, value);
        }

        public double? HeightOrMultiplier
        {
            get => this._heightOrMultiplier;
            set => this.Set(ref this._heightOrMultiplier, value);
        }

        public bool ShowToolTip
        {
            get => this._showToolTip;
            set => this.Set(ref this._showToolTip, value);
        }
    }
}