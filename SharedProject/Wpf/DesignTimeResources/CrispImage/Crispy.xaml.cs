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
            public ImageThemingColorChangedEventArgs(Color color) => Color = color;

            public Color Color { get; }
        }

        public static event EventHandler<ImageThemingColorChangedEventArgs> ImageThemingColorChanged;

        private static readonly bool s_isInDesignMode;
        private Image _image;

        static Crispy()
        {
            s_isInDesignMode = DesignModeHelper.IsInDesignMode;
            if (!s_isInDesignMode)
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
            if (!s_isInDesignMode)
            {
                return;
            }

            if (!(d is Crispy crispy))
            {
                return;
            }

            crispy.SetImageSource();
        }

        public ImageMoniker Moniker
        {
            get => (ImageMoniker)GetValue(Crispy.MonikerProperty);
            set => SetValue(Crispy.MonikerProperty, (object)value);
        }

        public Crispy()
        {
            InitializeComponent();
            if (!s_isInDesignMode)
            {
                SetupCrispImage();
                return;
            }

            SetupDesignTimeCrispImage();
        }

        private void SetupCrispImage()
        {
            // Create and set CrispImage as content
            var crispImage = new CrispImage();

            // Bind dependency properties from Crispy to CrispImage
            BindProperty(MonikerProperty, crispImage, CrispImage.MonikerProperty);
            BindProperty(WidthProperty, crispImage, CrispImage.WidthProperty);
            BindProperty(HeightProperty, crispImage, CrispImage.HeightProperty);

            Content = crispImage;
        }

        private void SetupDesignTimeCrispImage()
        {
            WeakEventManager<Crispy, ImageThemingColorChangedEventArgs>.AddHandler(null, nameof(Crispy.ImageThemingColorChanged), Crispy_ImageThemingColorChanged);

            SetImage();
            SetImageSource();
            Content = _image;
        }

        private void BindProperty(
            DependencyProperty sourceProperty,
            DependencyObject targetObject,
            DependencyProperty targetProperty
        )
        {
            var binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(sourceProperty),
                Mode = BindingMode.OneWay,
            };
            _ = BindingOperations.SetBinding(targetObject, targetProperty, binding);
        }

        /*
             could do this but it's not necessary
             protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        */

        private void SetImage()
        {
            _image = new Image();

            _ = BindingOperations.SetBinding(_image, Image.WidthProperty, new Binding(nameof(FrameworkElement.Width))
            {
                Source = this,
            });
            _ = BindingOperations.SetBinding(_image, Image.HeightProperty, new Binding(nameof(FrameworkElement.Height))
            {
                Source = this,
            });
        }

        private Color GetColor() => (Color)GetValue(ImageThemingUtilities.ImageBackgroundColorProperty);

        private void Crispy_ImageThemingColorChanged(object sender, ImageThemingColorChangedEventArgs e) => SetImageSource();

        private void SetImageSource()
        {
            var imageSource = (ImageSource)ImageLibraryLoader.Default.GetImage(Moniker, GetImageAttributes());
            _image.Source = imageSource;
        }

        private static uint ConvertColor(Color color) => (uint)(color.R | (color.G << 8) | (color.B << 16)); // | (color.A << 24));

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
                // imageAttributes.HighContrast = 1;
                Format = (uint)_UIDataFormat.DF_WPF,
                ImageType = (uint)_UIImageType.IT_Bitmap,
            };
            const _ImageAttributesFlags flags = _ImageAttributesFlags.IAF_RequiredFlags | _ImageAttributesFlags.IAF_Background; // others
            imageAttributes.Flags = BitConverter.ToUInt32(BitConverter.GetBytes((int)flags), 0);
            imageAttributes.StructSize = (int)Marshal.SizeOf<ImageAttributes>();
            imageAttributes.Background = ConvertColor(GetColor());
            imageAttributes.Dpi = GetDpi();

            // var scaleFactor = 1;
            imageAttributes.LogicalHeight = (int)Width;
            imageAttributes.LogicalWidth = (int)Height;
            /*
                device size
                 double num = dpi / 96.0 * scaleFactor;
                 DeviceSize = new Int16Size(logicalWidth * num, logicalHeight * num),
            */
            return imageAttributes;
        }
    }
}
