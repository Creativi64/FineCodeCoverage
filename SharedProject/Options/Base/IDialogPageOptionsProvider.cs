namespace FineCodeCoverage.Options
{
    public interface IDialogPageOptionsProvider<TOptions> where TOptions : class
    {
        void SaveSettingsToStorage();
        void LoadSettingsFromStorage();
        TOptions Options { get; }
    }

}
