using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;

namespace VsThemedDialogs
{
    public enum ThemedDialogWindowType
    {
        SmallMedium,
        Signature,
    }

    /// <summary>
    /// Displays a TextBlock with appropriate styling.
    /// </summary>
    public partial class ThemedDialogWindowTitle : UserControl
    {
        public ThemedDialogWindowTitle()
        {
            InitializeComponent();
            SetFontSizeStyleKey(DialogWindowType);
        }

        public ThemedDialogWindowType DialogWindowType
        {
            get => (ThemedDialogWindowType)GetValue(DialogWindowTypeProperty);
            set => SetValue(DialogWindowTypeProperty, value);
        }

        public static readonly DependencyProperty DialogWindowTypeProperty =
            DependencyProperty.Register(nameof(DialogWindowType), typeof(ThemedDialogWindowType), typeof(ThemedDialogWindowTitle), new PropertyMetadata(ThemedDialogWindowType.SmallMedium, OnDialogWindowTypeChanged));

        private static void OnDialogWindowTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ThemedDialogWindowTitle control))
            {
                return;
            }

            control.SetFontSizeStyleKey((ThemedDialogWindowType)e.NewValue);
        }

        private void SetFontSizeStyleKey(ThemedDialogWindowType type)
        {
            object key = null;
            switch (type)
            {
                case ThemedDialogWindowType.SmallMedium:
                    key = VsResourceKeys.TextBlockEnvironment200PercentFontSizeStyleKey;
                    break;
                case ThemedDialogWindowType.Signature:
                    key = VsResourceKeys.TextBlockEnvironment283PercentFontSizeStyleKey;
                    break;
            }

            TitleTextBlock.SetResourceReference(StyleProperty, key);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ThemedDialogWindowTitle), new PropertyMetadata(string.Empty));
    }
}
