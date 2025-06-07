namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal sealed class BuildStartEndArgs
    {
        public BuildStartEndArgs(bool isStart) => IsStart = isStart;

        public bool IsStart { get; }
    }
}
