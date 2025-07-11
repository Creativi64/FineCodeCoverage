namespace FineCodeCoverage.VSAbstractions.Dialogs
{
    public interface IDialogWindowService
    {
        void Show(object viewModel);
        void ShowModal(object viewModel);
    }
}
