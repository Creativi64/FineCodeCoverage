using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    /*
        The alternative to the BaseDialogWindow.xaml having a ContentControl
        to be populated from a DataTemplate by a derivation is for BaseDialogWindow to override the
        Control Template leaving the "hole" as a ContentPresenter. This would need to be done in code ***
        Derivations then could just use the Content property to set the content in xaml.
        
    */
    public partial class BaseDialogWindow : DialogWindow
    {
        public BaseDialogWindow(IDialogViewModel dialogViewModel)
        {
           
            this.DataContext = dialogViewModel;
            dialogViewModel.Done += (_, __) =>
            {
                this.Close();
            };
            
            InitializeComponent();
        }

    }

    public class BaseDialogWindow<T> : BaseDialogWindow where T : IDialogViewModel
    {
        protected T ViewModel { get; }
        public BaseDialogWindow(T dialogViewModel) : base(dialogViewModel)
        {
            ViewModel = dialogViewModel;
        }
    }


}
