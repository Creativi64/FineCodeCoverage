namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IFileLineCoverage
    {
        IFileLines GetLines(string filePath);
        void OutOfDate(string filePath);
    }
}
