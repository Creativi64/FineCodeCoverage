using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// Base dialog control for displaying custom content in a dialog.
    /// </summary>
    internal sealed partial class BaseDialogControl : UserControl
    {
        public BaseDialogControl() => InitializeComponent();

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object CustomContent
        {
            get => GetValue(CustomContentProperty);
            set => SetValue(CustomContentProperty, value);
        }

        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register(nameof(CustomContent), typeof(object), typeof(BaseDialogControl), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(BaseDialogControl), new PropertyMetadata(null));
    }
}
