namespace FineCodeCoverage.VSAbstractions.Dialogs
{
    public interface IDialogWindowService
    {
        void Show(object viewModel);
        bool? ShowModal(object viewModel);
    }
}
