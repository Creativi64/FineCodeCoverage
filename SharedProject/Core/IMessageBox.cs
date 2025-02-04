namespace FineCodeCoverage.Engine
{
    internal interface IMessageBox
    {
        void Show(string message);
        void ShowError(string error, string caption);
    }

}