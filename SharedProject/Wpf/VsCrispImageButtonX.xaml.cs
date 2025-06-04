using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Wpf
{
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
