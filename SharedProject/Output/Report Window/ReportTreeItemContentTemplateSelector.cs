using System.Windows.Controls;
using System.Windows;

namespace FineCodeCoverage.Output
{
    public class ReportTreeItemContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate SourceFileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SourceFileTreeItem)
            {
                return SourceFileTemplate;
            }
            return DefaultTemplate;  // Fallback to default template
        }
    }
}
