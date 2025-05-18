using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace FineCodeCoverage.Engine
{
    /*
        todo use VS message boxes
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.ivsuishell.showmessagebox?view=visualstudiosdk-2022
        IVsUIShell8 is 2022 only

        https://learn.microsoft.com/en-us/dotnet/api/system.windows.messagebox.show?view=windowsdesktop-9.0
    */
    [Export(typeof(IMessageBox))]
    [ExcludeFromCodeCoverage]
    internal class MessageBoxWrapper : IMessageBox
    {
        public void Show(string message)
        {
            MessageBox.Show(message);
        }

        public void ShowError(string error, string caption)
        {
            MessageBox.Show(error, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowWarning(string messageBoxText,string caption)
        {
            MessageBoxResult result = MessageBox.Show(
                messageBoxText,
                caption,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel); // <-- Cancel is the default

            return result == MessageBoxResult.OK;
        }
    }

}