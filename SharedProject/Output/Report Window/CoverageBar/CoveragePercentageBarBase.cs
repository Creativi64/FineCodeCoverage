using System.Windows;
using System.Windows.Media;
using FineCodeCoverage.Options;
using FineCodeCoverage.Wpf;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Output
{
    public abstract class CoveragePercentageBarBase : DependentPropertiesChangedUserControl<CoveragePercentageBarBase>
    {
        #region styling properties
        public CoveragePercentageBarDisplayParts DisplayParts
        {
            get => (CoveragePercentageBarDisplayParts)this.GetValue(DisplayPartsProperty);
            set => this.SetValue(DisplayPartsProperty, value);
        }

        public static readonly DependencyProperty DisplayPartsProperty =
            DependencyProperty.Register(
                nameof(DisplayParts),
                typeof(CoveragePercentageBarDisplayParts),
                typeof(CoveragePercentageBarBase),
                new PropertyMetadata(CoveragePercentageBarDisplayParts.Both));

        public bool UseSolidBrush
        {
            get => (bool)this.GetValue(UseSolidBrushProperty);
            set => this.SetValue(UseSolidBrushProperty, value);
        }

        public static readonly DependencyProperty UseSolidBrushProperty =
            DependencyProperty.Register(
                nameof(UseSolidBrush),
                typeof(bool),
                typeof(CoveragePercentageBarBase),
                new PropertyMetadata(true));

        public ILineBrushCreator LineBrushCreator
        {
            get => (ILineBrushCreator)this.GetValue(LineBrushCreatorProperty);
            set => this.SetValue(LineBrushCreatorProperty, value);
        }

        public static readonly DependencyProperty LineBrushCreatorProperty =
            DependencyProperty.Register(
                nameof(LineBrushCreator),
                typeof(ILineBrushCreator),
                typeof(CoveragePercentageBarBase),
                new PropertyMetadata(DefaultLineBrushCreator.Instance));

        #endregion

        #region coverage values
        public int? Partial
        {
            get => (int?)this.GetValue(PartialProperty); set => this.SetValue(PartialProperty, value);
        }

        public static readonly DependencyProperty PartialProperty =
            DependencyProperty.Register(nameof(Partial), typeof(int?), typeof(CoveragePercentageBarBase), new PropertyMetadata(null));

        public double Coverable
        {
            get => (double)this.GetValue(CoverableProperty); set => this.SetValue(CoverableProperty, value);
        }

        public static readonly DependencyProperty CoverableProperty =
            DependencyProperty.Register(nameof(Coverable), typeof(double), typeof(CoveragePercentageBarBase), new PropertyMetadata((double)0));

        public double Covered
        {
            get => (double)this.GetValue(CoveredProperty); set => this.SetValue(CoveredProperty, value);
        }

        public static readonly DependencyProperty CoveredProperty =
            DependencyProperty.Register(nameof(Covered), typeof(double), typeof(CoveragePercentageBarBase), new PropertyMetadata((double)0));
        #endregion

        public bool ShowToolTip
        {
            get => (bool)this.GetValue(ShowToolTipProperty); set => this.SetValue(ShowToolTipProperty, value);
        }

        public static readonly DependencyProperty ShowToolTipProperty =
            DependencyProperty.Register(nameof(ShowToolTip), typeof(bool), typeof(CoveragePercentageBarBase), new PropertyMetadata(true));

        public bool CoveredPercentageIsLeft
        {
            get => (bool)this.GetValue(CoveredPercentageIsLeftProperty); set => this.SetValue(CoveredPercentageIsLeftProperty, value);
        }

        public static readonly DependencyProperty CoveredPercentageIsLeftProperty =
            DependencyProperty.Register(nameof(CoveredPercentageIsLeft), typeof(bool), typeof(CoveragePercentageBarBase), new PropertyMetadata(false));

        #region colors

        public SolidColorBrush SingularPartBrush
        {
            get => (SolidColorBrush)this.GetValue(SingularPartBrushProperty); set => this.SetValue(SingularPartBrushProperty, value);
        }

        public static readonly DependencyProperty SingularPartBrushProperty =
            DependencyProperty.Register(nameof(SingularPartBrush), typeof(SolidColorBrush), typeof(CoveragePercentageBarBase), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty ThemedBackgroundColorProperty =
        DependencyProperty.Register(
            nameof(ThemedBackgroundColor),
            typeof(Color),
            typeof(CoveragePercentageBarBase),
            new PropertyMetadata(Colors.Transparent));

        public Color ThemedBackgroundColor
        {
            get => (Color)this.GetValue(ThemedBackgroundColorProperty);
            set => this.SetValue(ThemedBackgroundColorProperty, value);
        }

        public Color CoveredColor
        {
            get => (Color)this.GetValue(CoveredColorProperty); set => this.SetValue(CoveredColorProperty, value);
        }

        public static readonly DependencyProperty CoveredColorProperty =
            DependencyProperty.Register(nameof(CoveredColor), typeof(Color), typeof(CoveragePercentageBarBase), new PropertyMetadata(VisualStudioNotificationColors.Positive));

        public Color NotCoveredColor
        {
            get => (Color)this.GetValue(NotCoveredColorProperty); set => this.SetValue(NotCoveredColorProperty, value);
        }

        public static readonly DependencyProperty NotCoveredColorProperty =
            DependencyProperty.Register(nameof(NotCoveredColor), typeof(Color), typeof(CoveragePercentageBarBase), new PropertyMetadata(VisualStudioNotificationColors.Negative));

        #endregion

        public double? HeightOrMultiplier
        {
            get => (double?)this.GetValue(HeightOrMultiplierProperty); set => this.SetValue(HeightOrMultiplierProperty, value);
        }

        public static readonly DependencyProperty HeightOrMultiplierProperty =
            DependencyProperty.Register(nameof(HeightOrMultiplier), typeof(double?), typeof(CoveragePercentageBarBase), new PropertyMetadata(null));

        #region calculated properties

        [DependsOnProperty(nameof(ThemedBackgroundColor))]
        [DependsOnProperty(nameof(CoveredColor))]
        public SolidColorBrush CoveredBrush => this.GetPossiblyThemedBrush(true);

        [DependsOnProperty(nameof(ThemedBackgroundColor))]
        [DependsOnProperty(nameof(NotCoveredColor))]
        public SolidColorBrush NotCoveredBrush => this.GetPossiblyThemedBrush(false);

        private SolidColorBrush GetPossiblyThemedBrush(bool isCovered)
        {
            Color baseColor = isCovered ? this.CoveredColor : this.NotCoveredColor;
            SolidColorBrush brush = this.ThemedBackgroundColor == Colors.Transparent
                ? new SolidColorBrush(baseColor)
                : ImageThemingUtilitiesX.ThemeColorToSolidBrush(baseColor, this.ThemedBackgroundColor);
            brush.Freeze();
            return brush;
        }

        [DependsOnProperty(nameof(SingularPartBrush))]
        [DependsOnProperty(nameof(CoveredBrush))]
        [DependsOnProperty(nameof(UseSolidBrush))]
        [DependsOnProperty(nameof(DisplayParts))]
        [DependsOnProperty(nameof(LineBrushCreator))]
        public Brush StyledCoveredBrush => this.GetStyleBrush(CoveragePercentageBarDisplayParts.NotCovered, this.CoveredBrush);

        [DependsOnProperty(nameof(SingularPartBrush))]
        [DependsOnProperty(nameof(NotCoveredBrush))]
        [DependsOnProperty(nameof(UseSolidBrush))]
        [DependsOnProperty(nameof(DisplayParts))]
        [DependsOnProperty(nameof(LineBrushCreator))]
        public Brush StyledNotCoveredBrush => this.GetStyleBrush(CoveragePercentageBarDisplayParts.Covered, this.NotCoveredBrush);

        private Brush GetStyleBrush(CoveragePercentageBarDisplayParts otherStyle, SolidColorBrush solidColorBrush)
        {
            if (this.DisplayParts == otherStyle)
            {
                return Brushes.Transparent;
            }

            if (this.DisplayParts != CoveragePercentageBarDisplayParts.Both && this.SingularPartBrush.Color != Colors.Transparent)
            {
                solidColorBrush = this.SingularPartBrush;
            }

            return this.UseSolidBrush ? solidColorBrush : this.LineBrushCreator.Create(solidColorBrush.Color);
        }

        [DependsOnProperty(nameof(Covered))]
        [DependsOnProperty(nameof(Coverable))]
        public double Percentage => this.Coverable != 0 ? this.Covered / this.Coverable : 0;

        [DependsOnProperty(nameof(CoveredPercentageIsLeft))]
        [DependsOnProperty(nameof(DisplayParts))]
        public double RotationAngle
        {
            get
            {
                bool invertRotation = this.DisplayParts == CoveragePercentageBarDisplayParts.NotCovered;
                bool rotate = invertRotation ? !this.CoveredPercentageIsLeft : this.CoveredPercentageIsLeft;
                return rotate ? 0 : 180;
            }
        }

        [DependsOnProperty(nameof(Percentage))]
        [DependsOnProperty(nameof(DisplayParts))]
        public double ProgressBarValue => this.DisplayParts == CoveragePercentageBarDisplayParts.NotCovered ? 1 - this.Percentage : this.Percentage;

        [DependsOnProperty(nameof(DisplayParts))]
        [DependsOnProperty(nameof(StyledCoveredBrush))]
        [DependsOnProperty(nameof(StyledNotCoveredBrush))]
        public Brush ProgressBarBackgroundBrush => this.DisplayParts == CoveragePercentageBarDisplayParts.NotCovered ? this.StyledCoveredBrush : this.StyledNotCoveredBrush;

        [DependsOnProperty(nameof(DisplayParts))]
        [DependsOnProperty(nameof(StyledCoveredBrush))]
        [DependsOnProperty(nameof(StyledNotCoveredBrush))]
        public Brush ProgressBarForegroundBrush => this.DisplayParts == CoveragePercentageBarDisplayParts.NotCovered ? this.StyledNotCoveredBrush : this.StyledCoveredBrush;
        #endregion

        public CoverageTooltipViewModel TooltipModel => new CoverageTooltipViewModel(
            this.Percentage,
            this.Covered,
            this.Coverable,
            this.Partial
        );
    }
}