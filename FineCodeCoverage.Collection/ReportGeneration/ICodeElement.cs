using System.Collections.Generic;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface ICodeElement
    {
        CodeElementType CodeElementType { get; }

        string Name { get; }

        int StartLine { get; }

        string Path { get; }

        IReadOnlyList<ICoberturaLine> Lines { get; }

        int BlocksCovered { get; }

        int BlocksNotCovered { get; }

        int TotalBranches { get; }

        int BranchesCovered { get; }

        int CyclomaticComplexity { get; }

        int NPathComplexity { get; }

        decimal CrapScore { get; }
    }
}
