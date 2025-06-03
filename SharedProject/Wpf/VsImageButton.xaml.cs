using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public partial class VsImageButton : UserControl
    {
        public VsImageButton() => this.InitializeComponent();

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public object Image
        {
            get => this.GetValue(ImageProperty);
            set => this.SetValue(ImageProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(VsImageButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(object), typeof(VsImageButton), new PropertyMetadata(null));

    }
}