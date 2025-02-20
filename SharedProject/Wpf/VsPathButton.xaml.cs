using System.Windows;
using System.Windows.Media;

namespace FineCodeCoverage.Wpf
{
    public partial class VsPathButton : VsImageButtonX
    {

        public VsPathButton()
        {
            InitializeComponent();
        }

        public object Path
        {
            get { return (object)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(object), typeof(VsPathButton), new PropertyMetadata(null));


    }
}
