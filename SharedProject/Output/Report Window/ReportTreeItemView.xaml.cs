using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    public partial class ReportTreeItemView : UserControl
    {
        public ReportTreeItemView()
        {
            InitializeComponent();
        }


        public Brush IconBorderColor
        {
            get => (Brush)GetValue(IconBorderColorProperty);
            set => SetValue(IconBorderColorProperty, value);
        }
        public bool IsIconBorderColorTransparent
        {
            get => (bool)GetValue(IsIconBorderColorTransparentProperty);
            set => SetValue(IsIconBorderColorTransparentProperty, value);
        }

        public static readonly DependencyProperty IsIconBorderColorTransparentProperty =
    DependencyProperty.Register(nameof(IsIconBorderColorTransparent), typeof(bool), typeof(ReportTreeItemView), new PropertyMetadata(true));


        public static readonly DependencyProperty IconBorderColorProperty =
            DependencyProperty.Register(nameof(IconBorderColor), typeof(Brush), typeof(ReportTreeItemView), new PropertyMetadata(Brushes.Transparent, OnIconBorderColorChanged));

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(ReportTreeItemView), new PropertyMetadata(true));


        private static void OnIconBorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ReportTreeItemView)d;
            // Update IsIconBorderColorTransparent when IconBorderColor changes
            control.IsIconBorderColorTransparent = e.NewValue == Brushes.Transparent;
        }
    }
}
