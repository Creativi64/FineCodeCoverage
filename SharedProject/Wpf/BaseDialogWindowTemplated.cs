using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public class BaseDialogWindowTemplated : BaseViewModelDialogWindow
    {
        public BaseDialogWindowTemplated(IDialogViewModel dialogViewModel) : base(dialogViewModel)
        {
            Resources.AddFromExecutingAssembly("Wpf/BaseDialogWindowTemplatedResourceDictionary.xaml");
            var style = (Style)Resources[typeof(BaseDialogWindowTemplated)];
            Style = style;
        }
    }
}
