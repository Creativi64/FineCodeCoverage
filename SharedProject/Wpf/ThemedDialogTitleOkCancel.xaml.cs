using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FineCodeCoverage.Wpf
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
