using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// A user control that represents a button with an image, using a moniker for the image.
    /// </summary>
    public partial class VsCrispImageButton2 : UserControl
    {
        public VsCrispImageButton2() => InitializeComponent();

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command), typeof(ICommand), typeof(VsCrispImageButton2), new PropertyMetadata(null));

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
