using Microsoft.VisualStudio.Imaging.Interop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public partial class VsImageButton : UserControl
    {
        public VsImageButton()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(VsImageButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(object), typeof(VsImageButton), new PropertyMetadata(null));

    }

    public class VsCrispImageButton : VsImageButton
    {
        public static readonly DependencyProperty MonikerProperty =
DependencyProperty.Register(
nameof(Moniker),
typeof(ImageMoniker),
typeof(VsCrispImageButton));

        public ImageMoniker Moniker
        {
            get => (ImageMoniker)GetValue(MonikerProperty);
            set => SetValue(MonikerProperty, value);
        }

        

        public VsCrispImageButton()
        {
            var crispy = new Crispy();
            crispy.SetBinding(Crispy.MonikerProperty, new Binding(nameof(Moniker)) { Source = this });
            base.Image = crispy;
        }
    }
}
