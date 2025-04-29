using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System;
using System.Windows;

namespace FineCodeCoverage.Output
{
    public enum CoveragePercentageBarStyle { GreenRed, GreenLine, RedLine}
    public partial class CoveragePercentageBar : UserControl
    {
        private static Color DefaultCoveredColor = (Color)ColorConverter.ConvertFromString("#339933");
        private static Color DefaultNotCoveredColor = (Color)ColorConverter.ConvertFromString("#E51400");
        public CoveragePercentageBar()
        {
            InitializeComponent();
        }

        public CoveragePercentageBarStyle CoveragePercentageBarStyle
        {
            get { return (CoveragePercentageBarStyle)GetValue(CoveragePercentageBarStyleProperty); }
            set { SetValue(CoveragePercentageBarStyleProperty, value); }
        }

        public static readonly DependencyProperty CoveragePercentageBarStyleProperty =
            DependencyProperty.Register(nameof(CoveragePercentageBarStyle), typeof(CoveragePercentageBarStyle), typeof(CoveragePercentageBar), new PropertyMetadata(CoveragePercentageBarStyle.GreenRed));

        //public int  Partial
        //{
        //    get { return (int )GetValue(PartialProperty); }
        //    set { SetValue(PartialProperty, value); }
        //}

        //public static readonly DependencyProperty PartialProperty =
        //    DependencyProperty.Register(nameof(Partial), typeof(int ), typeof(CoveragePercentageBar), new PropertyMetadata(0));

        public double Coverable
        {
            get { return (double)GetValue(CoverableProperty); }
            set { SetValue(CoverableProperty, value); }
        }

        public static readonly DependencyProperty CoverableProperty =
            DependencyProperty.Register(nameof(Coverable), typeof(double), typeof(CoveragePercentageBar), new PropertyMetadata((double)0,CalculatePercentage));

        public double Covered
        {
            get { return (double)GetValue(CoveredProperty); }
            set { SetValue(CoveredProperty, value); }
        }

        public static readonly DependencyProperty CoveredProperty =
            DependencyProperty.Register(nameof(Covered), typeof(double), typeof(CoveragePercentageBar), new PropertyMetadata((double)0, CalculatePercentage));

        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register(nameof(Percentage), typeof(double), typeof(CoveragePercentageBar), new PropertyMetadata((double)0));

        private static void CalculatePercentage(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var coveragePercentageBar = d as CoveragePercentageBar;
            coveragePercentageBar.CalculatePercentage();
            // if no coverable then should style specifically
        }
        private void CalculatePercentage()
        {
            if(Coverable != 0)
            {
                Percentage = Covered / Coverable;
                var percentageRounded = Math.Round(Percentage * 100, 2);
                CoverageTooltip = $"{percentageRounded} % - {Covered} / {Coverable}";
            }
            else
            {
                CoverageTooltip = "No coverable";
            }
        }

        public string CoverageTooltip
        {
            get { return (string)GetValue(CoverageTooltipProperty); }
            set { SetValue(CoverageTooltipProperty, value); }
        }

        public static readonly DependencyProperty CoverageTooltipProperty =
            DependencyProperty.Register(nameof(CoverageTooltip), typeof(string), typeof(CoveragePercentageBar), new PropertyMetadata(""));

        public bool Invert
        {
            get { return (bool)GetValue(InvertProperty); }
            set { SetValue(InvertProperty, value); }
        }

        public static readonly DependencyProperty InvertProperty =
            DependencyProperty.Register(nameof(Invert), typeof(bool), typeof(CoveragePercentageBar), new PropertyMetadata(false));


        // just use ImageTheming ???? Call it ThemeBackgroundColor
        public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(CoveragePercentageBar),
            new PropertyMetadata(Colors.Transparent, OnBackgroundColorChanged));

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // if supply one then theme automatically
            var control = (CoveragePercentageBar)d;
            control.BackgroundColorChanged();
        }

        private void BackgroundColorChanged()
        {
            if(BackgroundColor == Colors.Transparent)
            {
                CoverageBorderThicknessActual = CoverageBarBorderThickness;
            }
            else
            {
                BorderThickness = new Thickness(0);
            }
            UpdateBrush(false);
            UpdateBrush(true);
        }

        public Thickness CoverageBarBorderThickness
        {
            get { return (Thickness)GetValue(CoverageBarBorderThicknessProperty); }
            set { SetValue(CoverageBarBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CoverageBarBorderThicknessProperty =
            DependencyProperty.Register(nameof(CoverageBarBorderThickness), typeof(Thickness), typeof(CoveragePercentageBar), new PropertyMetadata(new Thickness(0), CoverageBarBorderThicknessChanged));

        private static void CoverageBarBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CoveragePercentageBar).CoverageBarBorderThicknessChanged();
        }

        private void CoverageBarBorderThicknessChanged()
        {
            if (BackgroundColor == Colors.Transparent)
            {
                CoverageBorderThicknessActual = CoverageBarBorderThickness;
            }
        }

        private Thickness CoverageBorderThicknessActual
        {
            get { return (Thickness)GetValue(CoverageBorderThicknessActualProperty); }
            set { SetValue(CoverageBorderThicknessActualProperty, value); }
        }

        private static readonly DependencyProperty CoverageBorderThicknessActualProperty =
            DependencyProperty.Register(nameof(CoverageBorderThicknessActual), typeof(Thickness), typeof(CoveragePercentageBar), new PropertyMetadata(new Thickness(0)));

        public Brush CoverageBorderBrush
        {
            get { return (Brush)GetValue(CoverageBorderBrushProperty); }
            set { SetValue(CoverageBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty CoverageBorderBrushProperty =
            DependencyProperty.Register(nameof(CoverageBorderBrush), typeof(Brush), typeof(CoveragePercentageBar), new PropertyMetadata(Brushes.Transparent));

        public Color CoveredColor
        {
            get { return (Color)GetValue(CoveredColorProperty); }
            set { SetValue(CoveredColorProperty, value); }
        }

        public static readonly DependencyProperty CoveredColorProperty =
            DependencyProperty.Register(nameof(CoveredColor), typeof(Color), typeof(CoveragePercentageBar), new PropertyMetadata(DefaultCoveredColor, CoveredColorChanged));

        private static void CoveredColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CoveragePercentageBar).CoveredColorChanged();
        }
        private void CoveredColorChanged()
        {
            UpdateBrush(true);
        }
        public Color NotCoveredColor
        {
            get { return (Color)GetValue(NotCoveredColorProperty); }
            set { SetValue(NotCoveredColorProperty, value); }
        }

        public static readonly DependencyProperty NotCoveredColorProperty =
            DependencyProperty.Register(nameof(NotCoveredColor), typeof(Color), typeof(CoveragePercentageBar), new PropertyMetadata(DefaultNotCoveredColor, NotCoveredColorChanged));

        private static void NotCoveredColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CoveragePercentageBar).NotCoveredColorChanged();
        }
        private void NotCoveredColorChanged()
        {
            UpdateBrush(false);
        }

        private Brush CoveredBrush
        {
            get { return (Brush)GetValue(CoveredBrushProperty); }
            set { SetValue(CoveredBrushProperty, value); }
        }

        private static readonly DependencyProperty CoveredBrushProperty =
            DependencyProperty.Register(nameof(CoveredBrush), typeof(Brush), typeof(CoveragePercentageBar), new PropertyMetadata(new SolidColorBrush(DefaultCoveredColor)));

        private Brush NotCoveredBrush
        {
            get { return (Brush)GetValue(NotCoveredBrushProperty); }
            set { SetValue(NotCoveredBrushProperty, value); }
        }

        private static readonly DependencyProperty NotCoveredBrushProperty =
            DependencyProperty.Register(nameof(NotCoveredBrush), typeof(Brush), typeof(CoveragePercentageBar), new PropertyMetadata(new SolidColorBrush(DefaultNotCoveredColor)));

        private void UpdateBrush(bool isCovered)
        {
            var baseColor = isCovered ? CoveredColor : NotCoveredColor;
            SolidColorBrush brush = null;
            if ((BackgroundColor == Colors.Transparent))
            {
                brush = new SolidColorBrush(baseColor);
            }
            else
            {
                brush = ImageThemingUtilitiesX.ThemeColorToSolidBrush(baseColor, BackgroundColor);
            }
            brush.Freeze();

            if (isCovered)
                CoveredBrush = brush;
            else
                NotCoveredBrush = brush;
        }
    }
}

internal static class ImageThemingUtilitiesX
{
    public static Color ThemeColor(Color color, Color backgroundColor)
    {
        HslColor backgroundHsl = HslColor.FromColor(backgroundColor);
        var baseR = color.R;
        var baseG = color.G;
        var baseB = color.B;
        ImageThemingUtilities.ThemePixel(ref baseR, ref baseG, ref baseB, backgroundHsl);
        return Color.FromArgb(255, baseR, baseG, baseB);
    }
    public static SolidColorBrush ThemeColorToSolidBrush(Color color, Color backgroundColor)
    {
        return new SolidColorBrush(ThemeColor(color, backgroundColor));
    }
}
