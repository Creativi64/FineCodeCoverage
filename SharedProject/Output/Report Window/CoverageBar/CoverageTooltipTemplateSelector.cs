using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Output
{
    internal class CoverageTooltipTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FullTemplate { get; set; }

        public DataTemplate SimpleTemplate { get; set; }

        public DataTemplate NoneTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
            => item is CoverageTooltipViewModel vm
                ? vm.Coverable == 0 ?
                    NoneTemplate :
                    vm.Partial.HasValue ? FullTemplate : SimpleTemplate
                : base.SelectTemplate(item, container);
    }
}
