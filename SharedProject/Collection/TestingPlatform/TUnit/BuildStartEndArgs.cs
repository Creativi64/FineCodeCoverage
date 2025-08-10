namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal sealed class BuildStartEndArgs
    {
        public BuildStartEndArgs(bool isStart) => IsStart = isStart;

        public bool IsStart { get; }
    }
}
