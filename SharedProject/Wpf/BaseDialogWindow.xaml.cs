namespace FineCodeCoverage.Wpf
{
    public partial class BaseDialogWindow : BaseViewModelDialogWindow
    {
        public BaseDialogWindow(IDialogViewModel dialogViewModel):base(dialogViewModel)
        {
            InitializeComponent();
        }
    }
}
