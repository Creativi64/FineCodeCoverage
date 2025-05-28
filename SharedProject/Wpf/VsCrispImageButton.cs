using Microsoft.VisualStudio.Imaging.Interop;
using System.Windows;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
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
