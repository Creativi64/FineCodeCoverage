namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    internal interface IFileLineCoverage
    {
        IFileLines GetLines(string filePath);

        void OutOfDate(string filePath);
    }
}
