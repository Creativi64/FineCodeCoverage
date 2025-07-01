namespace FineCodeCoverage.Core.Initialization
{
    public interface IInitializeStatusProvider
    {
        InitializeStatus InitializeStatus { get; set; }

        string InitializeExceptionMessage { get; set; }
    }
}
