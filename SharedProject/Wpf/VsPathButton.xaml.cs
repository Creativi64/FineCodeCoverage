using System.Windows;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// todo.
    /// </summary>
    public partial class VsPathButton : VsImageButtonX
    {
        public VsPathButton() => InitializeComponent();

        public object Path
        {
            get => GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(object), typeof(VsPathButton), new PropertyMetadata(null));
    }
}
