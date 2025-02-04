using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public class BaseDialogWindowTemplated : DialogWindow
    {
        public BaseDialogWindowTemplated(IDialogViewModel dialogViewModel)
        {
            this.DataContext = dialogViewModel;
            dialogViewModel.Done += (_, __) =>
            {
                this.Close();
            };
            this.Resources.AddFromExecutingAssembly("Wpf/BaseDialogWindowTemplatedResourceDictionary.xaml");
            var style = (Style)this.Resources[typeof(BaseDialogWindowTemplated)];
            this.Style = style;
        }
    }
}
