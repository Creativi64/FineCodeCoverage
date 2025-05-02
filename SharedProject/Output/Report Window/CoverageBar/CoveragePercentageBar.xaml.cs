using System.Windows.Media;
using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
    public abstract class CoveragePercentageBarBase : DependentPropertiesChangedUserControl<CoveragePercentageBarBase>
    {
        #region styling properties
        public CoveragePercentageBarDisplayParts DisplayParts
        {
            get { return (CoveragePercentageBarDisplayParts)GetValue(DisplayPartsProperty); }
            set { SetValue(DisplayPartsProperty, value); }
        }

        public static readonly DependencyProperty DisplayPartsProperty =
            DependencyProperty.Register(nameof(DisplayParts), typeof(CoveragePercentageBarDisplayParts), typeof(CoveragePercentageBarBase), new PropertyMetadata(CoveragePercentageBarDisplayParts.Both));

        public bool UseSolidBrush
        {
            get { return (bool)GetValue(UseSolidBrushProperty); }
            set { SetValue(UseSolidBrushProperty, value); }
        }

        public static readonly DependencyProperty UseSolidBrushProperty =
            DependencyProperty.Register(nameof(UseSolidBrush), typeof(bool), typeof(CoveragePercentageBarBase), new PropertyMetadata(true));
        #endregion

        #region coverage values
        public int? Partial
        {
            get { return (int?)GetValue(PartialProperty); }
            set { SetValue(PartialProperty, value); }
        }

        public static readonly DependencyProperty PartialProperty =
            DependencyProperty.Register(nameof(Partial), typeof(int?), typeof(CoveragePercentageBarBase), new PropertyMetadata(null));

        public double Coverable
        {
            get { return (double)GetValue(CoverableProperty); }
            set { SetValue(CoverableProperty, value); }
        }

        public static readonly DependencyProperty CoverableProperty =
            DependencyProperty.Register(nameof(Coverable), typeof(double), typeof(CoveragePercentageBarBase), new PropertyMetadata((double)0));

        public double Covered
        {
            get { return (double)GetValue(CoveredProperty); }
            set { SetValue(CoveredProperty, value); }
        }

        public static readonly DependencyProperty CoveredProperty =
            DependencyProperty.Register(nameof(Covered), typeof(double), typeof(CoveragePercentageBarBase), new PropertyMetadata((double)0));
        #endregion

        public bool ShowToolTip
        {
            get { return (bool)GetValue(ShowToolTipProperty); }
            set { SetValue(ShowToolTipProperty, value); }
        }

        public static readonly DependencyProperty ShowToolTipProperty =
            DependencyProperty.Register(nameof(ShowToolTip), typeof(bool), typeof(CoveragePercentageBarBase), new PropertyMetadata(true));

        public bool CoveredPercentageIsLeft
        {
            get { return (bool)GetValue(CoveredPercentageIsLeftProperty); }
            set { SetValue(CoveredPercentageIsLeftProperty, value); }
        }

        public static readonly DependencyProperty CoveredPercentageIsLeftProperty =
            DependencyProperty.Register(nameof(CoveredPercentageIsLeft), typeof(bool), typeof(CoveragePercentageBarBase), new PropertyMetadata(false));

        #region colors

        public SolidColorBrush SingularPartBrush
        {
            get {   return (SolidColorBrush)GetValue(SingularPartBrushProperty); }
            set { SetValue(SingularPartBrushProperty, value); }
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
            get => (Color)GetValue(ThemedBackgroundColorProperty);
            set => SetValue(ThemedBackgroundColorProperty, value);
        }

        public Color CoveredColor
        {
            get { return (Color)GetValue(CoveredColorProperty); }
            set { SetValue(CoveredColorProperty, value); }
        }

        public static readonly DependencyProperty CoveredColorProperty =
            DependencyProperty.Register(nameof(CoveredColor), typeof(Color), typeof(CoveragePercentageBarBase), new PropertyMetadata(VisualStudioNotificationColors.Positive));

        public Color NotCoveredColor
        {
            get { return (Color)GetValue(NotCoveredColorProperty); }
            set { SetValue(NotCoveredColorProperty, value); }
        }

        public static readonly DependencyProperty NotCoveredColorProperty =
            DependencyProperty.Register(nameof(NotCoveredColor), typeof(Color), typeof(CoveragePercentageBarBase), new PropertyMetadata(VisualStudioNotificationColors.Negative));

        #endregion

        public double? HeightOrMultiplier
        {
            get { return (double?)GetValue(HeightOrMultiplierProperty); }
            set { SetValue(HeightOrMultiplierProperty, value); }
        }

        public static readonly DependencyProperty HeightOrMultiplierProperty =
            DependencyProperty.Register(nameof(HeightOrMultiplier), typeof(double?), typeof(CoveragePercentageBarBase), new PropertyMetadata(null));

        #region calculated properties

        [DependsOnProperty(nameof(ThemedBackgroundColor))]
        [DependsOnProperty(nameof(CoveredColor))]
        public SolidColorBrush CoveredBrush
        {
            get
            {
                return GetPossiblyThemedBrush(true);
            }
        }

        [DependsOnProperty(nameof(ThemedBackgroundColor))]
        [DependsOnProperty(nameof(NotCoveredColor))]
        public SolidColorBrush NotCoveredBrush
        {
            get
            {
                return GetPossiblyThemedBrush(false);
            }
        }

        private SolidColorBrush GetPossiblyThemedBrush(bool isCovered)
        {
            var baseColor = isCovered ? CoveredColor : NotCoveredColor;
            SolidColorBrush brush = null;
            if (ThemedBackgroundColor == Colors.Transparent)
            {
                brush = new SolidColorBrush(baseColor);
            }
            else
            {
                brush = ImageThemingUtilitiesX.ThemeColorToSolidBrush(baseColor, ThemedBackgroundColor);
            }
            brush.Freeze();
            return brush;
        }

        [DependsOnProperty(nameof(SingularPartBrush))]
        [DependsOnProperty(nameof(CoveredBrush))]
        [DependsOnProperty(nameof(UseSolidBrush))]
        [DependsOnProperty(nameof(DisplayParts))]
        public Brush StyledCoveredBrush => GetStyleBrush(CoveragePercentageBarDisplayParts.NotCovered, CoveredBrush);

        [DependsOnProperty(nameof(SingularPartBrush))]
        [DependsOnProperty(nameof(NotCoveredBrush))]
        [DependsOnProperty(nameof(UseSolidBrush))]
        [DependsOnProperty(nameof(DisplayParts))]
        public Brush StyledNotCoveredBrush => GetStyleBrush(CoveragePercentageBarDisplayParts.Covered, NotCoveredBrush);

        private Brush GetStyleBrush(CoveragePercentageBarDisplayParts otherStyle, SolidColorBrush solidColorBrush)
        {
            if (DisplayParts == otherStyle) {
                return Brushes.Transparent;
            }

            if(DisplayParts != CoveragePercentageBarDisplayParts.Both && SingularPartBrush.Color != Colors.Transparent)
            {
                solidColorBrush = SingularPartBrush;
            }
            return UseSolidBrush ? solidColorBrush : LineBrushCreator.Create(solidColorBrush);
        }

        [DependsOnProperty(nameof(Covered))]
        [DependsOnProperty(nameof(Coverable))]
        public double Percentage => Coverable != 0 ? Covered / Coverable : 0;

        [DependsOnProperty(nameof(Covered))]
        [DependsOnProperty(nameof(Coverable))]
        [DependsOnProperty(nameof(Partial))]
        public string CoverageTooltip
        {
            get
            {
                if (Coverable != 0)
                {
                    var percentageRounded = Math.Round(Percentage * 100, 2);
                    if (Partial.HasValue)
                    {
                        var partialValue = Partial.Value;
                        var uncovered = Coverable - Covered - Partial;
                        return
         $@"{percentageRounded} %
Covered     - {Covered}
Uncovered - {uncovered}
Partial       - {partialValue}
";
                    }
                    else
                    {
                        return $"{percentageRounded} % - {Covered} / {Coverable}";
                    }
                }
                else
                {
                    return "No coverable";
                }
            }
        }

        [DependsOnProperty(nameof(CoveredPercentageIsLeft))]
        [DependsOnProperty(nameof(DisplayParts))]
        public double RotationAngle
        {
            get
            {
                var invertRotation = DisplayParts == CoveragePercentageBarDisplayParts.NotCovered;
                var rotate = invertRotation ? !CoveredPercentageIsLeft : CoveredPercentageIsLeft;
                return rotate ? 0 : 180;
            }
        }

        [DependsOnProperty(nameof(Percentage))]
        [DependsOnProperty(nameof(DisplayParts))]
        public double ProgressBarValue => DisplayParts == CoveragePercentageBarDisplayParts.NotCovered ? 1 - Percentage : Percentage;

        [DependsOnProperty(nameof(DisplayParts))]
        [DependsOnProperty(nameof(StyledCoveredBrush))]
        [DependsOnProperty(nameof(StyledNotCoveredBrush))]
        public Brush ProgressBarBackgroundBrush => DisplayParts == CoveragePercentageBarDisplayParts.NotCovered ? StyledCoveredBrush : StyledNotCoveredBrush;

        [DependsOnProperty(nameof(DisplayParts))]
        [DependsOnProperty(nameof(StyledCoveredBrush))]
        [DependsOnProperty(nameof(StyledNotCoveredBrush))]
        public Brush ProgressBarForegroundBrush => DisplayParts == CoveragePercentageBarDisplayParts.NotCovered ? StyledNotCoveredBrush : StyledCoveredBrush;
        #endregion
    }

    public partial class CoveragePercentageBar : CoveragePercentageBarBase {
        public CoveragePercentageBar()
        {
            InitializeComponent();
        }
    }
}
