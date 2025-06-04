using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Utilities;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace FineCodeCoverage.Wpf
{
    public partial class Crispy : ContentControl
    {
        public class ImageThemingColorChangedEventArgs : EventArgs
        {
            public ImageThemingColorChangedEventArgs(Color color) => this.Color = color;

            public Color Color { get; }
        }

        public static event EventHandler<ImageThemingColorChangedEventArgs> ImageThemingColorChanged;
        private static readonly bool isInDesignMode;
        private Image _image;

        static Crispy()
        {
            isInDesignMode = DesignModeHelper.IsInDesignMode;
            if (!isInDesignMode)
            {
                return;
            }

            ImageThemingUtilities.ImageBackgroundColorProperty.OverrideMetadata(
                typeof(Crispy),
                new FrameworkPropertyMetadata((_, args) => ImageThemingColorChanged?.Invoke(
                    null,
                    new ImageThemingColorChangedEventArgs((Color)args.NewValue))));
        }

        

        public static readonly DependencyProperty MonikerProperty = CrispImage.MonikerProperty.AddOwner(
            typeof(Crispy),
            new FrameworkPropertyMetadata(OnDependencyPropertyChanged)
            );

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!isInDesignMode) return;

            if (d is Crispy crispy)
            {
                crispy.SetImageSource();
            }
        }

        public ImageMoniker Moniker
        {
            get => (ImageMoniker)this.GetValue(Crispy.MonikerProperty);
            set => this.SetValue(Crispy.MonikerProperty, (object)value);
        }
        public Crispy()
        {
            this.InitializeComponent();
            if (!isInDesignMode)
            {
                this.SetupCrispImage();
                return;
            }

            this.SetupDesignTimeCrispImage();
        }

        private void SetupCrispImage()
        {
            // Create and set CrispImage as content
            var crispImage = new CrispImage();

            // Bind dependency properties from Crispy to CrispImage
            this.BindProperty(MonikerProperty, crispImage, CrispImage.MonikerProperty);
            this.BindProperty(WidthProperty, crispImage, CrispImage.WidthProperty);
            this.BindProperty(HeightProperty, crispImage, CrispImage.HeightProperty);

            this.Content = crispImage;
        }

        private void SetupDesignTimeCrispImage()
        {
            WeakEventManager<Crispy, ImageThemingColorChangedEventArgs>.AddHandler(null, nameof(Crispy.ImageThemingColorChanged), this.Crispy_ImageThemingColorChanged);

            this.SetImage();
            this.SetImageSource();
            this.Content = this._image;
        }

        private void BindProperty(DependencyProperty sourceProperty,
                          DependencyObject targetObject,
                          DependencyProperty targetProperty)
        {
            var binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(sourceProperty),
                Mode = BindingMode.OneWay
            };
            _ = BindingOperations.SetBinding(targetObject, targetProperty, binding);
        }

        // could do this but it's not necessary
        // protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)

        private void SetImage()
        {
            this._image = new Image();

            _ = BindingOperations.SetBinding(this._image, Image.WidthProperty, new Binding(nameof(FrameworkElement.Width))
            {
                Source = this,
            });
            _ = BindingOperations.SetBinding(this._image, Image.HeightProperty, new Binding(nameof(FrameworkElement.Height))
            {
                Source = this,
            });
        }

        private Color GetColor() => (Color)this.GetValue(ImageThemingUtilities.ImageBackgroundColorProperty);

        private void Crispy_ImageThemingColorChanged(object sender, ImageThemingColorChangedEventArgs e) => this.SetImageSource();

        private void SetImageSource()
        {
            var imageSource = (ImageSource)ImageLibraryLoader.Default.GetImage(this.Moniker, this.GetImageAttributes());
            this._image.Source = imageSource;
        }

        private static uint ConvertColor(Color color) => (uint)(color.R | (color.G << 8) | (color.B << 16));// | (color.A << 24));

        private static int GetDpi()
        {
            double dpi;
            try
            {
                dpi = DpiAwareness.SystemDpiX;
            }
            catch
            {
                dpi = 96.0;
            }

            return (int)dpi;
        }

        private ImageAttributes GetImageAttributes()
        {
            var imageAttributes = new ImageAttributes
            {
                //imageAttributes.HighContrast = 1;
                Format = (uint)_UIDataFormat.DF_WPF,
                ImageType = (uint)_UIImageType.IT_Bitmap
            };
            const _ImageAttributesFlags flags = _ImageAttributesFlags.IAF_RequiredFlags | _ImageAttributesFlags.IAF_Background;// others
            imageAttributes.Flags = BitConverter.ToUInt32(BitConverter.GetBytes((int)flags), 0);
            imageAttributes.StructSize = (int)Marshal.SizeOf<ImageAttributes>();
            imageAttributes.Background = ConvertColor(this.GetColor());
            imageAttributes.Dpi = GetDpi();

            //var scaleFactor = 1;  

            imageAttributes.LogicalHeight = (int)this.Width;
            imageAttributes.LogicalWidth = (int)this.Height;
            /*
                device size
                 double num = dpi / 96.0 * scaleFactor;
                 DeviceSize = new Int16Size(logicalWidth * num, logicalHeight * num),
            */
            return imageAttributes;
        }
    }
}