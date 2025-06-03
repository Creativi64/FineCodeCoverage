using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Readme
{
    public class HyperlinkContainsImageConverter : ValueConverter<object, bool>
    {
        protected override bool Convert(object value, object parameter, CultureInfo culture)
            => value is InlineCollection source &&
            source.FirstOrDefault() is InlineUIContainer inlineUiContainer &&
            inlineUiContainer.Child is Button button &&
            button.Content is Image;
    }
}