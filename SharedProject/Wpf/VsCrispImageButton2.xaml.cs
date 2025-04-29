using Microsoft.VisualStudio.Imaging.Interop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public partial class VsCrispImageButton2 : UserControl
    {
        public VsCrispImageButton2()
        {
            InitializeComponent();
        }

        public ICommand  Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(VsCrispImageButton2), new PropertyMetadata(null));

        public static readonly DependencyProperty ImageMonikerProperty =
DependencyProperty.Register(
    nameof(ImageMoniker),
    typeof(ImageMoniker),
    typeof(VsCrispImageButton2),
    new PropertyMetadata(default(ImageMoniker)));

        public ImageMoniker ImageMoniker
        {
            get => (ImageMoniker)GetValue(ImageMonikerProperty);
            set => SetValue(ImageMonikerProperty, value);
        }
    }
}
