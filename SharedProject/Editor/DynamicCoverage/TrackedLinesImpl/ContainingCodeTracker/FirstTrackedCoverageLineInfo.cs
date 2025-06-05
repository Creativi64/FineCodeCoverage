namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class FirstTrackedCoverageLineInfo
    {
        public FirstTrackedCoverageLineInfo(
            int originalLineNumber,
            IDynamicCoberturaLine dynamicCoberturaLine)
        {
            OriginalLineNumber = originalLineNumber;
            DynamicCoberturaLine = dynamicCoberturaLine;
        }

        public int OriginalLineNumber { get; }

        public IDynamicCoberturaLine DynamicCoberturaLine { get; }
    }
}
