namespace FineCodeCoverage.Wpf
{
    public class BaseViewModelDialogWindow : GlitchFixingDialogWindow
    {
        public BaseViewModelDialogWindow(IDialogViewModel dialogViewModel)
        {
            this.DataContext = dialogViewModel;
            dialogViewModel.Done += (_, __) =>
            {
                this.Close();
            };

        }
    }
}
