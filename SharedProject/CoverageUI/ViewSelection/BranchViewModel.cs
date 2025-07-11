namespace FineCodeCoverage.Output
{
    internal sealed class BranchViewModel
    {
        public BranchViewModel(string branchName)
        {
            BranchName = branchName;
            Display = branchName;
        }

        public string BranchName { get; }

        public string Display { get; }

        public override string ToString() => Display;
    }
}
