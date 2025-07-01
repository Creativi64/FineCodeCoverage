using System.Collections.Generic;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IClass
    {
        string DisplayName { get; }

        IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; }

        IReadOnlyList<ICodeElement> CodeElements { get; }
    }
}
