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
        private class FontsAndColorsCoverageBarColours
        {
            public FontsAndColorsCoverageBarColours(Color coveredColor, Color notCoveredColor)
            {
                CoveredColor = coveredColor;
                NotCoveredColor = notCoveredColor;
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
            _coverageColoursProvider = coverageColoursProvider;
            _ = eventAggregator.AddListener(this);
            reportOptionsProvider.OptionsChanged += SetOptions;
            ReportOptions reportOptions = reportOptionsProvider.Get();
            _lastCoveragePercentageBarColorsFromFontsAndColors = !reportOptions.CoveragePercentageUseColorsFromFontsAndColors;
            SetOptions(reportOptions);
        }

        private void SetOptions(ReportOptions reportOptions)
        {
            DisplayParts = reportOptions.CoveragePercentageDisplayParts;
            CoveredPercentageIsLeft = reportOptions.CoveragePercentageCoveredIsLeft;
            IsThemed = reportOptions.CoveragePercentageIsThemed;
            UseSolidBrush = reportOptions.CoveragePercentageUseSolidBrush;
            UseContrastedThemeWhenSingularDisplay = reportOptions.CoveragePercentageUseContrastedThemeWhenSingularDisplay;
            HeightOrMultiplier = reportOptions.CoveragePercentageHeightOrMultiplier;
            ShowToolTip = reportOptions.CoveragePercentageShowTooltip;
            if (reportOptions.CoveragePercentageUseColorsFromFontsAndColors == _lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                return;
            }

            SetCoverageColours(reportOptions.CoveragePercentageUseColorsFromFontsAndColors);
            _lastCoveragePercentageBarColorsFromFontsAndColors = reportOptions.CoveragePercentageUseColorsFromFontsAndColors;
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
            FontsAndColorsCoverageBarColours fontsAndColorsCoverageBarColours = GetFontsAndColorsCoverageColours();
            NotCoveredColor = fontsAndColorsCoverageBarColours.NotCoveredColor;
            CoveredColor = fontsAndColorsCoverageBarColours.CoveredColor;
        }

        private FontsAndColorsCoverageBarColours GetFontsAndColorsCoverageColours()
        {
            if (_fontsAndColorsCoverageBarColours == null || _coverageColoursDirty)
            {
                ICoverageColours coverageColours = _coverageColoursProvider.GetCoverageColours();
                Color coveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.Covered).Background;
                Color notCoveredColor = coverageColours.GetColour(Editor.DynamicCoverage.DynamicCoverageType.NotCovered).Background;
                _fontsAndColorsCoverageBarColours = new FontsAndColorsCoverageBarColours(coveredColor, notCoveredColor);
                _coverageColoursDirty = false;
            }

            return _fontsAndColorsCoverageBarColours;
        }

        public void Handle(CoverageColoursChangedMessage message)
        {
            _coverageColoursDirty = true;
            if (!_lastCoveragePercentageBarColorsFromFontsAndColors)
            {
                return;
            }

            SetCoverageColoursFromFontsAndColors();
        }

        public CoveragePercentageBarDisplayParts DisplayParts
        {
            get => _displayParts;
            set => Set(ref _displayParts, value);
        }

        public bool CoveredPercentageIsLeft
        {
            get => _coveredPercentageIsLeft;
            set => Set(ref _coveredPercentageIsLeft, value);
        }

        public bool IsThemed
        {
            get => _isThemed;
            set => Set(ref _isThemed, value);
        }

        public Color CoveredColor
        {
            get => _coveredColor;
            set => Set(ref _coveredColor, value);
        }

        public Color NotCoveredColor
        {
            get => _notCoveredColor;
            set => Set(ref _notCoveredColor, value);
        }

        public bool UseSolidBrush
        {
            get => _useSolidBrush;
            set => Set(ref _useSolidBrush, value);
        }

        public bool UseContrastedThemeWhenSingularDisplay
        {
            get => _useContrastThemeWhenSingularDisplay;
            set => Set(ref _useContrastThemeWhenSingularDisplay, value);
        }

        public double? HeightOrMultiplier
        {
            get => _heightOrMultiplier;
            set => Set(ref _heightOrMultiplier, value);
        }

        public bool ShowToolTip
        {
            get => _showToolTip;
            set => Set(ref _showToolTip, value);
        }
    }
}
