using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public abstract class BaseDialogWindowTemplated : BaseViewModelDialogWindow
    {
        protected BaseDialogWindowTemplated(IDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            Resources.AddFromExecutingAssembly("Wpf/BaseDialogWindowTemplatedResourceDictionary.xaml");
            var style = (Style)Resources[typeof(BaseDialogWindowTemplated)];
            Style = style;
        }
    }
}
