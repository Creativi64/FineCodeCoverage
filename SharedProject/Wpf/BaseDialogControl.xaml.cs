using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    internal partial class BaseDialogControl : UserControl
    {
        public BaseDialogControl()
        {
            this.InitializeComponent();
        }

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public object CustomContent
        {
            get => this.GetValue(CustomContentProperty);
            set => this.SetValue(CustomContentProperty, value);
        }

        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register(nameof(CustomContent), typeof(object), typeof(BaseDialogControl), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(BaseDialogControl), new PropertyMetadata(null));
    }
}
