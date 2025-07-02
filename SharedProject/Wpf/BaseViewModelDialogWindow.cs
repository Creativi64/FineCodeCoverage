namespace FineCodeCoverage.Wpf
{
    public abstract class BaseViewModelDialogWindow : GlitchFixingDialogWindow
    {
        protected BaseViewModelDialogWindow(IDialogViewModel dialogViewModel)
        {
            DataContext = dialogViewModel;
            dialogViewModel.Done += (_, __) => Close();
        }
    }
}
