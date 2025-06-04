namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class BuildStartEndArgs
    {
        public BuildStartEndArgs(bool isStart) => IsStart = isStart;

        public bool IsStart { get; }
    }
}
