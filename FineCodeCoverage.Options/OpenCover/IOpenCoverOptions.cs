namespace FineCodeCoverage.Options
{
    public interface IOpenCoverOptions
    {
        string OpenCoverCustomPath { get; set; }

        OpenCoverRegister OpenCoverRegister { get; set; }

        string OpenCoverTarget { get; set; }

        string OpenCoverTargetArgs { get; set; }
    }
}
