using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public partial class VsImageButtonX : ContentControl
    {
        static VsImageButtonX() => DefaultStyleKeyProperty.OverrideMetadata(typeof(VsImageButtonX),
            new FrameworkPropertyMetadata(typeof(VsImageButtonX)));

        public VsImageButtonX()
        {
            this.Resources.AddFromExecutingAssembly("Wpf/VsImageButtonResourceDictionary.xaml");
            var style = (Style)this.Resources[typeof(VsImageButtonX)];
            this.Style = style;
        }

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(VsImageButtonX), new PropertyMetadata(null));

    }
}
