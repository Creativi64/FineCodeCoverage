using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public class BaseDialogWindowTemplated : BaseViewModelDialogWindow
    {
        public BaseDialogWindowTemplated(IDialogViewModel dialogViewModel) : base(dialogViewModel)
        {

            this.Resources.AddFromExecutingAssembly("Wpf/BaseDialogWindowTemplatedResourceDictionary.xaml");
            var style = (Style)this.Resources[typeof(BaseDialogWindowTemplated)];
            this.Style = style;
        }
    }
}