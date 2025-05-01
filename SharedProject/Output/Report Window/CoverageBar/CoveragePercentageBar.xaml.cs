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
        public CoveragePercentageBarStyle CoveragePercentageBarStyle
        {
            get { return (CoveragePercentageBarStyle)GetValue(CoveragePercentageBarStyleProperty); }
            set { SetValue(CoveragePercentageBarStyleProperty, value); }
        }

        public static readonly DependencyProperty CoveragePercentageBarStyleProperty =
            DependencyProperty.Register(nameof(CoveragePercentageBarStyle), typeof(CoveragePercentageBarStyle), typeof(CoveragePercentageBarBase), new PropertyMetadata(CoveragePercentageBarStyle.Percent));

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

        public bool CoveredPercentageLeft
        {
            get { return (bool)GetValue(CoveredPercentageLeftProperty); }
            set { SetValue(CoveredPercentageLeftProperty, value); }
        }

        public static readonly DependencyProperty CoveredPercentageLeftProperty =
            DependencyProperty.Register(nameof(CoveredPercentageLeft), typeof(bool), typeof(CoveragePercentageBarBase), new PropertyMetadata(false));

        #region colors

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
            DependencyProperty.Register(nameof(CoveredColor), typeof(Color), typeof(CoveragePercentageBarBase), new PropertyMetadata(VisualStudioNotificationColors.Green));

        public Color NotCoveredColor
        {
            get { return (Color)GetValue(NotCoveredColorProperty); }
            set { SetValue(NotCoveredColorProperty, value); }
        }

        public static readonly DependencyProperty NotCoveredColorProperty =
            DependencyProperty.Register(nameof(NotCoveredColor), typeof(Color), typeof(CoveragePercentageBarBase), new PropertyMetadata(VisualStudioNotificationColors.Red));

        #endregion



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

        [DependsOnProperty(nameof(CoveredBrush))]
        [DependsOnProperty(nameof(UseSolidBrush))]
        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        public Brush StyledCoveredBrush => GetStyleBrush(CoveragePercentageBarStyle.NotCovered, CoveredBrush);


        [DependsOnProperty(nameof(NotCoveredBrush))]
        [DependsOnProperty(nameof(UseSolidBrush))]
        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        public Brush StyledNotCoveredBrush => GetStyleBrush(CoveragePercentageBarStyle.Covered, NotCoveredBrush);

        private Brush GetStyleBrush(CoveragePercentageBarStyle otherStyle, SolidColorBrush solidColorBrush)
            => CoveragePercentageBarStyle == otherStyle ? Brushes.Transparent : UseSolidBrush ? solidColorBrush : LineBrushCreator.Create(solidColorBrush);

        [DependsOnProperty(nameof(Covered))]
        [DependsOnProperty(nameof(Coverable))]
        public double Percentage => Coverable != 0 ? Covered / Coverable : 0;

        [DependsOnProperty(nameof(Percentage))]
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

        [DependsOnProperty(nameof(CoveredPercentageLeft))]
        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        public double RotationAngle
        {
            get
            {
                var invertRotation = CoveragePercentageBarStyle == CoveragePercentageBarStyle.NotCovered;
                var rotate = invertRotation ? !CoveredPercentageLeft : CoveredPercentageLeft;
                return rotate ? 0 : 180;
            }
        }

        [DependsOnProperty(nameof(Percentage))]
        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        public double ProgressBarValue => CoveragePercentageBarStyle == CoveragePercentageBarStyle.NotCovered ? 1 - Percentage : Percentage;

        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        [DependsOnProperty(nameof(StyledCoveredBrush))]
        [DependsOnProperty(nameof(StyledNotCoveredBrush))]
        public Brush ProgressBarBackgroundBrush => CoveragePercentageBarStyle == CoveragePercentageBarStyle.NotCovered ? StyledCoveredBrush : StyledNotCoveredBrush;

        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        [DependsOnProperty(nameof(StyledCoveredBrush))]
        [DependsOnProperty(nameof(StyledNotCoveredBrush))]
        public Brush ProgressBarForegroundBrush => CoveragePercentageBarStyle == CoveragePercentageBarStyle.NotCovered ? StyledNotCoveredBrush : StyledCoveredBrush;
        #endregion
    }

    public partial class CoveragePercentageBar : CoveragePercentageBarBase {
        public CoveragePercentageBar()
        {
            InitializeComponent();
        }
    }
}
