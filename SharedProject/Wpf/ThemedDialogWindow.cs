using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Wpf
{
    [System.Windows.Markup.ContentProperty(nameof(BodyContent))]
    public class ThemedDialogWindow : DialogWindow
    {
        static ThemedDialogWindow() => DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ThemedDialogWindow),
                new FrameworkPropertyMetadata(typeof(ThemedDialogWindow)));

        public static readonly DependencyProperty BodyContentProperty =
            DependencyProperty.Register(
            nameof(BodyContent),
            typeof(object),
            typeof(ThemedDialogWindow),
            new FrameworkPropertyMetadata(null));

        public object BodyContent
        {
            get => GetValue(BodyContentProperty);
            set => SetValue(BodyContentProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.FixSizeToContentWidthAndHeightBlackBars();
        }
    }
}
