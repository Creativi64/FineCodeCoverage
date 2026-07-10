using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace VsThemedDialogs
{
    [ContentProperty(nameof(Body))]
    public partial class ThemedDialogTitleOkCancel : UserControl
    {
        public ThemedDialogTitleOkCancel() => InitializeComponent();

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }



        public string OkButtonText
        {
            get => (string)GetValue(OkButtonTextProperty);
            set => SetValue(OkButtonTextProperty, value);
        }

        public static readonly DependencyProperty OkButtonTextProperty =
            DependencyProperty.Register(nameof(OkButtonText), typeof(string), typeof(ThemedDialogTitleOkCancel), new PropertyMetadata("Ok"));



        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ThemedDialogTitleOkCancel), new PropertyMetadata(string.Empty));

        public object Body
        {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(nameof(Body), typeof(object), typeof(ThemedDialogTitleOkCancel), new PropertyMetadata(null));

    }
}
