using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Wpf
{
    public partial class VsCrispImageButton2 : UserControl
    {
        public VsCrispImageButton2() => this.InitializeComponent();

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
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
            get => (ImageMoniker)this.GetValue(ImageMonikerProperty);
            set => this.SetValue(ImageMonikerProperty, value);
        }
    }
}
