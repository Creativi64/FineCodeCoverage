namespace FineCodeCoverage.Readme
{
    public interface IShowReadMeService
    {
        void Show();
        bool HasShown { get; }
        event System.EventHandler Shown;
    }
}