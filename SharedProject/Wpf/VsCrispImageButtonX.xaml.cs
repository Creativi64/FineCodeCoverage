using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// A user control that represents a button with an image, using a moniker for the image.
    /// </summary>
    public partial class VsCrispImageButtonX : VsImageButtonX
    {
        public static readonly DependencyProperty MonikerProperty =
            DependencyProperty.Register(
            nameof(Moniker),
            typeof(ImageMoniker),
            typeof(VsCrispImageButtonX));

        public ImageMoniker Moniker
        {
            get => (ImageMoniker)GetValue(MonikerProperty);
            set => SetValue(MonikerProperty, value);
        }

        public VsCrispImageButtonX() => InitializeComponent();
    }
}
