using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    public class ThemedDialogCloseButton : Button
    {
        static ThemedDialogCloseButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedDialogCloseButton), new FrameworkPropertyMetadata(typeof(ThemedDialogCloseButton)));

        protected override void OnClick()
        {
            if (Command != null)
            {
                base.OnClick();
                return;
            }

            Window.GetWindow(this)?.Close();
        }
    }
}
