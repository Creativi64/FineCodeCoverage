using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;
using Color = System.Windows.Media.Color;
using System.Windows.Data;

namespace FineCodeCoverage.Wpf
{
    public partial class Crispy : ContentControl
    {
        public class ImageThemingColorChangedArgs : EventArgs
        {
            public ImageThemingColorChangedArgs(Color color)
            {
                Color = color;
            }

            public Color Color { get; }
        }
        
        public static event EventHandler<ImageThemingColorChangedArgs> ImageThemingColorChanged;
        private static readonly bool isInDesignMode;
        static Crispy()
        {
            isInDesignMode = DesignModeHelper.IsInDesignMode;
            if (!isInDesignMode)
            {
                return;
            }
            ImageThemingUtilities.ImageBackgroundColorProperty.OverrideMetadata(typeof(Crispy), new FrameworkPropertyMetadata((d, args) =>
            {
                ImageThemingColorChanged?.Invoke(null, new ImageThemingColorChangedArgs((Color)args.NewValue));
            }));
        }
        
        private Image image;


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
            InitializeComponent();
            if (!isInDesignMode)
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
            WeakEventManager<Crispy, ImageThemingColorChangedArgs>.AddHandler(null, nameof(Crispy.ImageThemingColorChanged), Crispy_ImageThemingColorChanged);

            SetImage();
            SetImageSource();
            this.Content = image;
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
            BindingOperations.SetBinding(targetObject, targetProperty, binding);
        }

        // could do this but it's not necessary
        // protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)


        private void SetImage()
        {
            image = new Image { };

            BindingOperations.SetBinding(image, Image.WidthProperty, new Binding(nameof(FrameworkElement.Width))
            {
                Source = this,
            });
            BindingOperations.SetBinding(image, Image.HeightProperty, new Binding(nameof(FrameworkElement.Height))
            {
                Source = this,
            });
        }

        
        private Color GetColor()
        {
            return (Color)GetValue(ImageThemingUtilities.ImageBackgroundColorProperty);
        }

        private void Crispy_ImageThemingColorChanged(object sender, ImageThemingColorChangedArgs e)
        {
            SetImageSource();
        }

        private void SetImageSource()
        {
            var imageSource = (ImageSource)ImageLibraryLoader.Default.GetImage(Moniker, GetImageAttributes());
            image.Source = imageSource;
        }

        private uint ConvertColor(Color color)
        {
            return (uint)(color.R | (color.G << 8) | (color.B << 16));// | (color.A << 24));
        }

        private ImageAttributes GetImageAttributes()
        {
            var imageAttributes = new ImageAttributes();
            //imageAttributes.HighContrast = 1;
            imageAttributes.Format = (uint)_UIDataFormat.DF_WPF;
            imageAttributes.ImageType = (uint)_UIImageType.IT_Bitmap;
            _ImageAttributesFlags flags = _ImageAttributesFlags.IAF_RequiredFlags | _ImageAttributesFlags.IAF_Background;// others
            imageAttributes.Flags = (uint)flags;
            imageAttributes.StructSize = (int)Marshal.SizeOf<ImageAttributes>();
            
            imageAttributes.Background = ConvertColor(GetColor());
            double defaultDpi = 0;
            try
            {
                defaultDpi = DpiAwareness.SystemDpiX;
            }
            catch
            {
                defaultDpi = 96.0;
            }
            imageAttributes.Dpi = (int)defaultDpi;
            
            //var scaleFactor = 1;  

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
