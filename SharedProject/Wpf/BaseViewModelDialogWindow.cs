namespace FineCodeCoverage.Wpf
{
    public class BaseViewModelDialogWindow : GlitchFixingDialogWindow
    {
        public BaseViewModelDialogWindow(IDialogViewModel dialogViewModel)
        {
            DataContext = dialogViewModel;
            dialogViewModel.Done += (_, __) => Close();
        }
    }
}
