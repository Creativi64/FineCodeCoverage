using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public partial class VsPathButton : VsImageButtonX
    {

        public VsPathButton() => this.InitializeComponent();

        public object Path
        {
            get => (object)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(object), typeof(VsPathButton), new PropertyMetadata(null));
    }
}