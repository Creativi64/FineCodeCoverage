using System.Windows;
using System.Windows.Data;
using Microsoft.VisualStudio.Imaging.Interop;

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
            get => (ImageMoniker)this.GetValue(MonikerProperty);
            set => this.SetValue(MonikerProperty, value);
        }

        public VsCrispImageButton()
        {
            var crispy = new Crispy();
            _ = crispy.SetBinding(Crispy.MonikerProperty, new Binding(nameof(this.Moniker)) { Source = this });
            base.Image = crispy;
        }
    }
}
