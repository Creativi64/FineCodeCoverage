namespace FineCodeCoverage.Wpf
{
    /// <summary>
    /// base dialog window.
    /// </summary>
    public partial class BaseDialogWindow : BaseViewModelDialogWindow
    {
        public BaseDialogWindow(IDialogViewModel dialogViewModel)
            : base(dialogViewModel)
            => InitializeComponent();
    }
}
