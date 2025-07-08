using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    public class ThemedDialogCloseButton : Button
    {
        static ThemedDialogCloseButton() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedDialogCloseButton), new FrameworkPropertyMetadata((object)typeof(ThemedDialogCloseButton)));
    }
}
