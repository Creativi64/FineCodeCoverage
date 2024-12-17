namespace FineCodeCoverage.Readme
{
    public interface IReadMeService
    {
        void ShowReadMe();
        bool HasShownReadMe { get; }
        event System.EventHandler ReadMeShown;
    }
}
