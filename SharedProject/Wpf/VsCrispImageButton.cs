using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FineCodeCoverage.Wpf
{
    public class VsCrispImageButton : Button
    {
        public VsCrispImageButton()
        {
            this.Style = Application.Current.Resources[VsResourceKeys.ButtonStyleKey] as Style;
            this.MinWidth = 26;
            this.Width = 26;
            this.Height = 23;
            this.Padding = new Thickness(0);
            ImageThemingUtilities.SetImageBackgroundColor(this, (this.Background as SolidColorBrush).Color);
        }

        public static readonly DependencyProperty ImageMonikerProperty =
        DependencyProperty.Register(
            nameof(ImageMoniker),
            typeof(ImageMoniker),
            typeof(VsCrispImageButton),
            new PropertyMetadata(default(ImageMoniker), OnImageMonikerChanged));

        public ImageMoniker ImageMoniker
        {
            get => (ImageMoniker)GetValue(ImageMonikerProperty);
            set => SetValue(ImageMonikerProperty, value);
        }

        private static void OnImageMonikerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VsCrispImageButton button)
            {
                button.UpdateImageContent();
            }
        }

        private void UpdateImageContent()
        {
            var image = new CrispImage { Moniker = ImageMoniker, Width = 16, Height = 16 };
            Content = image;
        }
    }
}
