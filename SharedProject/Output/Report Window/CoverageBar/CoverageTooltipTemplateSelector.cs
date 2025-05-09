using System.Windows.Controls;
using System.Windows;

namespace FineCodeCoverage.Output
{
    internal class CoverageTooltipTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FullTemplate { get; set; }
        public DataTemplate SimpleTemplate { get; set; }
        public DataTemplate NoneTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CoverageTooltipViewModel vm)
            {
                if (vm.Coverable == 0)
                    return NoneTemplate;
                if (vm.Partial.HasValue)
                    return FullTemplate;
                return SimpleTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }

}
