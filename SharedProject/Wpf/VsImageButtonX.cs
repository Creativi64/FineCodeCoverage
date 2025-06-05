using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public partial class VsImageButtonX : ContentControl
    {
        static VsImageButtonX() => DefaultStyleKeyProperty.OverrideMetadata(
            typeof(VsImageButtonX),
            new FrameworkPropertyMetadata(typeof(VsImageButtonX))
        );

        public VsImageButtonX()
        {
            Resources.AddFromExecutingAssembly("Wpf/VsImageButtonResourceDictionary.xaml");
            var style = (Style)Resources[typeof(VsImageButtonX)];
            Style = style;
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(VsImageButtonX), new PropertyMetadata(null));

    }
}
