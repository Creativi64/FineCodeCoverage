namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// base dialog window.
    /// </summary>
    public abstract partial class BaseDialogWindow : BaseViewModelDialogWindow
    {
        protected BaseDialogWindow(IDialogViewModel dialogViewModel)
            : base(dialogViewModel)
            => InitializeComponent();
    }
}
