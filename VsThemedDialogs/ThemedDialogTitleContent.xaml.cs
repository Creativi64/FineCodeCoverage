using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace VsThemedDialogs
{
    /// <summary>
    /// Displays themed dialog title and body content with 24 left margin.
    /// </summary>
    [ContentProperty(nameof(Body))]
    public partial class ThemedDialogTitleContent : UserControl
    {
        public ThemedDialogTitleContent() => InitializeComponent();

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ThemedDialogTitleContent), new PropertyMetadata(string.Empty));

        public object Body
        {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(nameof(Body), typeof(object), typeof(ThemedDialogTitleContent), new PropertyMetadata(null));
    }
}
